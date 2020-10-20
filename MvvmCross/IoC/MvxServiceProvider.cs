using System;
using Microsoft.Extensions.DependencyInjection;
using MvvmCross.Base;

namespace MvvmCross.IoC
{
    public class MvxServiceProvider : MvxSingleton<IServiceProvider>, IServiceProvider
    {
        public static IServiceProvider Initialize(IServiceCollection collection, ServiceProviderOptions options)
        {
            if (Instance != null)
            {
                return Instance;
            }

            // create a new ioc container - it will register itself as the singleton
            // ReSharper disable ObjectCreationAsStatement
            new MvxServiceProvider(collection, options);

            // ReSharper restore ObjectCreationAsStatement
            return Instance;
        }
        
        private readonly ServiceProvider _provider;

        protected MvxServiceProvider(IServiceCollection collection, ServiceProviderOptions options)
        {
            _provider = collection.BuildServiceProvider(options);
        }
        
        public object GetService(Type serviceType) => _provider.GetService(serviceType);
    }
}
