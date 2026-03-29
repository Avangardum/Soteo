// using Microsoft.Extensions.DependencyInjection;
// using Soteo.Shared.Enums;
//
// namespace Soteo.Client;
//
// public sealed class ServiceProvider : Node, IServiceProvider
// {
//     private IServiceProvider _innerProvider = null!;
//     
//     public static ServiceProvider Instance { get; private set; } = null!;
//     
//     public override void _Ready()
//     {
//         if (Instance != null) throw new InvalidOperationException();
//         Instance = this;
//         
//         var serviceCollection = new ServiceCollection();
//         RegisterServices(serviceCollection);
//         
//         _innerProvider = serviceCollection.BuildServiceProvider();
//     }
//     
//     public void RegisterServices(IServiceCollection serviceCollection)
//     {
//         foreach (Type type in TypeLocator.MessageHandlerTypes.Values) serviceCollection.AddTransient(type);
//         
//         var masterServerLink = GetNode<MasterServerLink>("/root/MasterServerLink");
//         serviceCollection.AddSingleton<IMessageSender>(masterServerLink);
//     }
//
//     public object? GetService(Type serviceType) => _innerProvider.GetService(serviceType);
// }