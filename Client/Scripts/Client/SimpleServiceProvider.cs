using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Shared.Extensions;

namespace Soteo.Client;

/// <summary>
/// Custom IServiceProvider implementation. The default implementation is avoided because it uses threading, which
/// breaks the engine. https://github.com/godotengine/godot/issues/118124
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
        ServiceDescriptor? descriptor = GetDescriptor(serviceType);
        return GetService(descriptor, recursionDepth);
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
    
    private ServiceDescriptor? GetDescriptor(Type serviceType)
    {
        List<ServiceDescriptor> descriptors = _descriptorsByType.GetOrDefault(serviceType) ?? [];
        if (descriptors.Count > 1)
            throw new InvalidOperationException($"Several registrations of service {serviceType}");
        return descriptors.SingleOrDefault();
    }
    
    private IEnumerable<object?> GetAllServices(Type type, int recursionDepth)
    {
        if (_descriptorsByType.TryGetValue(type, out List<ServiceDescriptor> descriptors))
            return descriptors.Select(it => GetService(it, recursionDepth)).ToList();
        return [];
    }

    private bool TryGetCachedService(ServiceDescriptor descriptor, out object? cachedService)
    {
        switch (descriptor.Lifetime)
        {
            case ServiceLifetime.Singleton:
                return _singletonCache.TryGetValue(descriptor, out cachedService);
            case ServiceLifetime.Scoped:
                if (_scopedCache == null)
                    throw new InvalidOperationException("Can't resolve scoped service from the root provider");
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
        ConstructorInfo[] constructors = descriptor.ImplementationType.GetConstructors();
        if (constructors.Length > 1)
            throw new InvalidOperationException($"{descriptor.ImplementationType} has multiple public constructors");
        if (constructors.Length == 0)
            throw new InvalidOperationException($"{descriptor.ImplementationType} doesn't have a public constructor");
        object[] args = constructors[0].GetParameters()
            .Select(it => recursionLimiter.GetService(it.ParameterType) ?? throw new InvalidOperationException(
                $"Failed to resolve {it.ParameterType} to create {descriptor.ImplementationType}"))
            .ToArray();
        return Activator.CreateInstance(descriptor.ImplementationType, args);
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