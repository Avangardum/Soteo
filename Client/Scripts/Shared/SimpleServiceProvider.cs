using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Shared.Extensions;

namespace Soteo.Shared;

// todo consider to switching to third party implementation
/// <summary>
/// Custom IServiceProvider implementation. This class is kept general, fully usable through IServiceProvider, without
/// anything Soteo specific. The default implementation is avoided because it uses threading, which
/// breaks the engine in web export. https://github.com/godotengine/godot/issues/118124
/// </summary>
public sealed class SimpleServiceProvider : IServiceProvider, IServiceScopeFactory, IDisposable
{
    private class RecursionLimiter(SimpleServiceProvider provider, int recursionDepth) : IServiceProvider
    {
        private const int MaxRecursionDepth = 50;
        
        public object? GetService(Type serviceType)
        {
            if (recursionDepth > MaxRecursionDepth) throw new InvalidOperationException($"Max recursion depth" +
                $"exceeded while resolving {serviceType}. There is likely a circular dependency.");
            return provider.GetService(serviceType, recursionDepth);
        }
    }
    
    private class Scope(SimpleServiceProvider provider, SimpleServiceProvider parent) : IServiceScope
    {
        public void Dispose()
        {
            provider.Dispose();
            parent._childScopeProviders.Remove(provider);
        }

        public IServiceProvider ServiceProvider => provider;
    }

    private readonly Dictionary<Type, List<ServiceDescriptor>> _descriptorsByType;
    private readonly Dictionary<ServiceDescriptor, object?> _singletonCache;
    private readonly Dictionary<ServiceDescriptor, object?>? _scopedCache;
    private readonly List<SimpleServiceProvider> _childScopeProviders = [];
    private readonly List<IDisposable> _disposableServices = [];
    private bool _isDisposed;

    /// <summary>
    /// Root provider constructor
    /// </summary>
    public SimpleServiceProvider(IServiceCollection serviceCollection)
    {
        _descriptorsByType = serviceCollection.GroupBy(it => it.ServiceType)
            .ToDictionary(it => it.Key, it => it.ToList());
        _singletonCache = [];
    }
    
    /// <summary>
    /// Scope provider constructor
    /// </summary>
    private SimpleServiceProvider
    (
        Dictionary<Type, List<ServiceDescriptor>> descriptorsByType,
        Dictionary<ServiceDescriptor, object?> singletonCache
    )
    {
        _descriptorsByType = descriptorsByType;
        _singletonCache = singletonCache;
        _scopedCache = [];
    }
    
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _disposableServices.ForEach(it => it.Dispose());
        _childScopeProviders.ForEach(it => it.Dispose());
    }
    
    public IServiceScope CreateScope()
    {
        if (_isDisposed) throw new ObjectDisposedException("Service provider is disposed");
        var scopeProvider = new SimpleServiceProvider(_descriptorsByType, _singletonCache);
        _childScopeProviders.Add(scopeProvider);
        return new Scope(scopeProvider, this);
    }

    public object? GetService(Type serviceType) => GetService(serviceType, 0);
    
    private object? GetService(Type serviceType, int recursionDepth)
    {
        if (_isDisposed) throw new ObjectDisposedException("Service provider is disposed");
        if (serviceType == typeof(IServiceProvider)) return this;
        if (serviceType == typeof(IServiceScopeFactory)) return this;
        if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return GetAllServices(serviceType.GenericTypeArguments.Single(), recursionDepth);
        IReadOnlyList<ServiceDescriptor> descriptors = GetDescriptors(serviceType);
        if (descriptors.Count > 1) throw new InvalidOperationException($"Multiple registrations for {serviceType}");
        return GetService(descriptors.SingleOrDefault(), recursionDepth);
    }
    
    private object? GetService(ServiceDescriptor? descriptor, int recursionDepth)
    {
        if (descriptor == null) return null;
        if (TryGetCachedService(descriptor, out object? cachedService)) return cachedService;
        object? newService = CreateService(descriptor, recursionDepth);
        if (newService is IDisposable disposable) _disposableServices.Add(disposable);
        CacheService(descriptor, newService);
        return newService;
    }
    
    private IReadOnlyList<ServiceDescriptor> GetDescriptors(Type serviceType)
    {
        List<ServiceDescriptor> descriptors = _descriptorsByType.GetOrDefault(serviceType)?.ToList() ?? [];
        if (serviceType.IsConstructedGenericType &&
            _descriptorsByType.TryGetValue(serviceType.GetGenericTypeDefinition(), out var openGenericDescriptors))
        {
            descriptors.AddRange(openGenericDescriptors.Select(it => CloseOpenGenericDescriptor(it, serviceType)));
        }
        return descriptors;
    }
    
    /// <summary>
    /// Convert a ServiceDescriptor with open generic types to a ServiceDescriptor with closed generic types, using
    /// specified closed generic service type.<br />
    /// Example: f(ServiceDescriptor { ServiceType = IEnumerable&lt;&gt;, ImplementationType = List&lt;&gt;
    /// Lifetime = l }, IEnumerable&lt;string&gt;) = ServiceDescriptor { ServiceType = IEnumerable&lt;string&gt;,
    /// ImplementationType = List&lt;string&gt; Lifetime = l }
    /// </summary>
    private ServiceDescriptor CloseOpenGenericDescriptor
    (
        ServiceDescriptor openGenericDescriptor,
        Type serviceClosedGenericType
    )
    {
        // Open generic descriptors are implemented by closing them on each resolution, this won't work with caching
        if (openGenericDescriptor.Lifetime != ServiceLifetime.Transient)
            throw new NotSupportedException("Only transient open generic descriptors are supported");
        
        if (openGenericDescriptor.ServiceType != serviceClosedGenericType.GetGenericTypeDefinition())
        {
            throw new ArgumentException(
                $"{openGenericDescriptor.ServiceType} is not the generic definition of {serviceClosedGenericType}");
        }
        
        if (openGenericDescriptor.ImplementationType == null)
            throw new ArgumentException("Open generic descriptors must have ImplementationType");
        
        Type implementationClosedGenericType =
            openGenericDescriptor.ImplementationType.MakeGenericType(serviceClosedGenericType.GenericTypeArguments);
        
        return new ServiceDescriptor(serviceClosedGenericType, implementationClosedGenericType,
            openGenericDescriptor.Lifetime);
    }
    
    private IEnumerable<object?> GetAllServices(Type type, int recursionDepth)
    {
        return GetDescriptors(type).Select(it => GetService(it, recursionDepth)).ToList();
    }

    private bool TryGetCachedService(ServiceDescriptor descriptor, out object? cachedService)
    {
        switch (descriptor.Lifetime)
        {
            case ServiceLifetime.Singleton:
                return _singletonCache.TryGetValue(descriptor, out cachedService);
            case ServiceLifetime.Scoped:
                if (_scopedCache == null) throw new InvalidOperationException(
                    $"Can't resolve scoped service {descriptor.ServiceType} from the root provider");
                return _scopedCache.TryGetValue(descriptor, out cachedService);
            case ServiceLifetime.Transient:
                cachedService = null;
                return false;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public object? CreateService(ServiceDescriptor descriptor, int recursionDepth)
    {
        if (descriptor.ImplementationInstance != null) return descriptor.ImplementationInstance;
        
        var recursionLimiter = new RecursionLimiter(this, recursionDepth + 1);
        if (descriptor.ImplementationFactory != null) return descriptor.ImplementationFactory(recursionLimiter);
        
        if (descriptor.ImplementationType == null) return null;
        ConstructorInfo[] constructors = descriptor.ImplementationType.GetConstructors()
            .OrderByDescending(it => it.GetParameters().Length)
            .ThenBy(it => it.GetParameters().Select(p => p.ParameterType.FullName).JoinToString(""))
            .ToArray();
        
        foreach (ConstructorInfo constructor in constructors)
        {
            object?[] arguments = constructor.GetParameters()
                .Select(it => recursionLimiter.GetService(it.ParameterType))
                .ToArray();
            if (arguments.All(it => it != null))
                return Activator.CreateInstance(descriptor.ImplementationType, arguments);
        }
        
        throw new InvalidOperationException(
            $"Couldn't find a fitting constructor to create {descriptor.ImplementationType}");
    }
    
    private void CacheService(ServiceDescriptor descriptor, object? service)
    {
        switch (descriptor.Lifetime)
        {
            case ServiceLifetime.Singleton:
                _singletonCache[descriptor] = service;
                break;
            case ServiceLifetime.Scoped:
                _scopedCache![descriptor] = service;
                break;
            case ServiceLifetime.Transient:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}