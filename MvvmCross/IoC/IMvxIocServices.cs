using System;
using Microsoft.Extensions.DependencyInjection;

namespace MvvmCross.IoC
{
    public interface IMvxIocServices
    {
        IServiceCollection ServiceCollection { get; }
        IServiceProvider ServiceProvider { get; }

        IServiceProvider BuildServiceProvider(ServiceProviderOptions options);
        void InvalidateServiceProvider();
    }
}
