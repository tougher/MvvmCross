// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MvvmCross.Base;
using MvvmCross.Core;
using MvvmCross.Exceptions;
using MvvmCross.Logging;

namespace MvvmCross.IoC
{
    /// <summary>
    /// Singleton IoC Provider.
    ///
    /// Delegates to the <see cref="MvxIoCContainer"/> implementation
    /// </summary>
    [Obsolete("Use Microsoft.Extensions.DependencyInjection instead")]
    public class MvxIoCProvider
        : MvxSingleton<IMvxIoCProvider>, IMvxIoCProvider
    {
        public static IMvxIoCProvider Initialize(IMvxIocServices iocServices, IMvxIocOptions options = null)
        {
            if (Instance != null)
            {
                return Instance;
            }

            // create a new ioc container - it will register itself as the singleton
            // ReSharper disable ObjectCreationAsStatement
            new MvxIoCProvider(iocServices, options ?? new MvxIocOptions());

            // ReSharper restore ObjectCreationAsStatement
            return Instance;
        }
        
        private readonly IMvxIocServices _iocServices;
        private readonly IMvxIocOptions _options;

        private readonly IServiceCollection _collection;
        public IServiceCollection ServiceCollection => _collection ?? _iocServices.ServiceCollection;
        
        private IServiceProvider _provider;
        public IServiceProvider ServiceProvider
        {
            get => _provider ?? _iocServices.ServiceProvider;
        }

        public IServiceProvider BuildServiceProvider(ServiceProviderOptions options)
        {
            if (_iocServices != null)
            {
                if (ServiceCollection == null) return null;
                ServiceCollection.BuildServiceProvider(options);
                return ServiceProvider;
            }
            
            if (_collection == null) return null;
            _provider = _collection.BuildServiceProvider(options);
            return _provider;
        }

        public void InvalidateServiceProvider()
        {
            if (_iocServices != null)
            {
                if (ServiceProvider == null) return;
                _iocServices.InvalidateServiceProvider();
                return;
            }
            
            _provider = null;
        }

        protected MvxIoCProvider(IMvxIocServices iocServices, IMvxIocOptions options)
        {
            _iocServices = iocServices;
            _options = options;
        }
        
        protected MvxIoCProvider(IServiceCollection collection, IServiceProvider provider)
        {
            _collection = collection;
            _provider = provider;
        }

        public bool CanResolve<T>()
            where T : class
        {
            CheckCollection();
            return ServiceCollection.Any(s => s.ServiceType == typeof(T));
        }

        public bool CanResolve(Type t)
        {
            CheckCollection();
            return ServiceCollection.Any(s => s.ServiceType == t);
        }

        public bool TryResolve<T>(out T resolved)
            where T : class
        {
            try
            {
                CheckProvider();
                resolved = ServiceProvider.GetService<T>();
                return resolved != null;
            }
            catch (Exception e)
            {
                if (e.InnerException != null) MvxLog.Instance?.Error(e.InnerException.Message);
            }

            resolved = null;
            return false;
        }

        public bool TryResolve(Type type, out object resolved)
        {
            try
            {
                CheckProvider();
                resolved = ServiceProvider.GetService(type);
                return resolved != null;
            }
            catch (AggregateException e)
            {
                if (e.InnerException != null) MvxLog.Instance?.Error(e.InnerException.Message);
            }

            resolved = null;
            return false;
        }

        public T Resolve<T>()
            where T : class
        {
            try
            {
                CheckProvider();
                
                return ServiceProvider.GetRequiredService<T>();
            }
            catch
            {
                throw new MvxIoCResolveException("Failed to resolve type {0}", typeof(T).FullName);
            }
        }

        public object Resolve(Type t)
        {
            try
            {
                CheckProvider();
                return ServiceProvider.GetRequiredService(t);
            }
            catch
            {
                throw new MvxIoCResolveException("Failed to resolve type {0}", t.FullName);
            }
        }

        public T GetSingleton<T>()
            where T : class
        {
            try{
                CheckProvider();
                return ServiceProvider.GetRequiredService<T>();
            }
            catch
            {
                throw new MvxIoCResolveException("Failed to resolve type {0}", typeof(T).FullName);
            }
        }

        public object GetSingleton(Type t)
        {
            try{
                CheckProvider();
                return ServiceProvider.GetRequiredService(t);
            }
            catch
            {
                throw new MvxIoCResolveException("Failed to resolve type {0}", t.FullName);
            }
        }

        public T Create<T>()
            where T : class
        {
            try{
                CheckProvider();
                return ServiceProvider.GetRequiredService<T>();
            }
            catch
            {
                throw new MvxIoCResolveException("Failed to resolve type {0}", typeof(T).FullName);
            }
        }

        public object Create(Type t)
        {
            try{
                CheckProvider();
                return ServiceProvider.GetRequiredService(t);
            }
            catch
            {
                throw new MvxIoCResolveException("Failed to resolve type {0}", t.FullName);
            }
        }

        public void RegisterType<TInterface, TToConstruct>()
            where TInterface : class
            where TToConstruct : class, TInterface
        {
            CheckCollection();
            ServiceCollection.AddTransient<TInterface, TToConstruct>();
        }

        public void RegisterType<TInterface>(Func<TInterface> constructor)
            where TInterface : class
        {
            CheckCollection();
            ServiceCollection.AddTransient<TInterface>(sp => constructor());
        }

        public void RegisterType(Type t, Func<object> constructor)
        {
            //_provider.RegisterType(t, constructor);
            CheckCollection();
            ServiceCollection.AddTransient(t, sp => constructor());
        }

        public void RegisterType(Type interfaceType, Type constructType)
        {
            //_provider.RegisterType(interfaceType, constructType);
            CheckCollection();
            ServiceCollection.AddTransient(interfaceType, constructType);
        }

        public void RegisterSingleton<TInterface>(TInterface theObject)
            where TInterface : class
        {
            //_provider.RegisterSingleton(theObject);
            CheckCollection();
            ServiceCollection.AddSingleton(theObject);
        }

        public void RegisterSingleton(Type interfaceType, object theObject)
        {
            //_provider.RegisterSingleton(interfaceType, theObject);
            CheckCollection();
            ServiceCollection.AddSingleton(interfaceType, theObject);
        }

        public void RegisterSingleton<TInterface>(Func<TInterface> theConstructor)
            where TInterface : class
        {
            //_provider.RegisterSingleton(theConstructor);
            CheckCollection();
            var it = typeof(TInterface);
            ServiceCollection.AddSingleton<TInterface>(sp =>
            {
                var ctor = theConstructor();
                return ctor;
            });
        }

        public void RegisterSingleton(Type interfaceType, Func<object> theConstructor)
        {
            //_provider.RegisterSingleton(interfaceType, theConstructor);
            CheckCollection();
            ServiceCollection.AddSingleton(interfaceType, sp => theConstructor());
        }

        public T IoCConstruct<T>()
            where T : class
        {
            CheckProvider();
            return ActivatorUtilities.CreateInstance<T>(ServiceProvider);
        }

        public virtual object IoCConstruct(Type type)
        {
            CheckProvider();

            try
            {
                return ActivatorUtilities.CreateInstance(ServiceProvider, type);
            }
            catch (InvalidOperationException e)
            {
                throw new MvxIoCResolveException(e, "Failed to construct {0}", type.Name);
            }
        }

        public T IoCConstruct<T>(IDictionary<string, object> arguments) where T : class
        {
            //return _provider.IoCConstruct<T>(arguments);
            CheckProvider();
            var selectedConstructor = typeof(T).FindApplicableConstructor(arguments);

            if (selectedConstructor == null)
            {
                throw new MvxIoCResolveException("Failed to find constructor for type {0}", typeof(T).FullName);
            }

            var parameters = GetIoCParameterValues(typeof(T), selectedConstructor, arguments);
            return ActivatorUtilities.CreateInstance<T>(ServiceProvider, parameters);
        }

        public T IoCConstruct<T>(params object[] arguments) where T : class
        {
            //return _provider.IoCConstruct<T>(arguments);
            CheckProvider();
            
            try
            {
                return ActivatorUtilities.CreateInstance<T>(ServiceProvider, arguments);
            }
            catch (InvalidOperationException e)
            {
                throw new MvxIoCResolveException(e, "Failed to construct {0}", typeof(T).Name);
            }
        }

        public T IoCConstruct<T>(object arguments) where T : class
        {
            //return _provider.IoCConstruct<T>(arguments);
            CheckProvider();
            var argsDict = arguments.ToPropertyDictionary();
            var selectedConstructor = typeof(T).FindApplicableConstructor(argsDict);

            if (selectedConstructor == null)
            {
                throw new MvxIoCResolveException("Failed to find constructor for type {0}", typeof(T).FullName);
            }

            var parameters = GetIoCParameterValues(typeof(T), selectedConstructor, argsDict);
            return ActivatorUtilities.CreateInstance<T>(ServiceProvider, parameters);
        }

        public object IoCConstruct(Type type, IDictionary<string, object> arguments = null)
        {
            //return _provider.IoCConstruct(type, arguments);
            CheckProvider();
            var selectedConstructor = type.FindApplicableConstructor(arguments);

            if (selectedConstructor == null)
            {
                throw new MvxIoCResolveException("Failed to find constructor for type {0}", type.FullName);
            }

            var parameters = GetIoCParameterValues(type, selectedConstructor, arguments);
            return ActivatorUtilities.CreateInstance(ServiceProvider, type, parameters);
        }

        public object IoCConstruct(Type type, object arguments)
        {
            //return _provider.IoCConstruct(type, arguments);
            CheckProvider();
            var argsDict = arguments.ToPropertyDictionary();
            var selectedConstructor = type.FindApplicableConstructor(argsDict);

            if (selectedConstructor == null)
            {
                throw new MvxIoCResolveException("Failed to find constructor for type {0}", type.FullName);
            }

            var parameters = GetIoCParameterValues(type, selectedConstructor, argsDict);
            return ActivatorUtilities.CreateInstance(ServiceProvider, type, parameters);
        }

        public object IoCConstruct(Type type, params object[] arguments)
        {
            //return _provider.IoCConstruct(type, arguments);
            CheckProvider();
            return ActivatorUtilities.CreateInstance(ServiceProvider, type, arguments);
        }

        public void CallbackWhenRegistered<T>(Action action)
        {
            CallbackWhenRegistered(typeof(T), action);
        }

        public void CallbackWhenRegistered(Type type, Action action)
        {
            if (!(ServiceCollection is MvxServiceCollectionCallbackDecorator callbackCollection))
            {
                return;
            }
                
            if (!CanResolve(type))
            {
                if (callbackCollection.Waiters.TryGetValue(type, out var actions))
                {
                    actions.Add(action);
                }
                else
                {
                    actions = new List<Action> { action };
                    callbackCollection.Waiters[type] = actions;
                }
                return;
            }

            // if we get here then the type is already registered - so call the aciton immediately
            action();
        }

        public void CleanAllResolvers()
        {
            //_provider.CleanAllResolvers();
            CheckCollection();
            ServiceCollection.Clear();
        }

        public IMvxIoCProvider CreateChildContainer()
        {
            throw new NotImplementedException("This is not supported anymore. Please use Microsoft.Extensions.DependencyInjection instead.");
        }
        
        private bool TryResolveParameter(Type type, ParameterInfo parameterInfo, out object parameterValue)
        {
            if (!TryResolve(parameterInfo.ParameterType, out parameterValue))
            {
                if (parameterInfo.IsOptional)
                {
                    parameterValue = Type.Missing;
                }
                else
                {
                    throw new MvxIoCResolveException(
                        "Failed to resolve parameter for parameter {0} of type {1} when creating {2}. You may pass it as an argument",
                        parameterInfo.Name,
                        parameterInfo.ParameterType.Name,
                        type.FullName);
                }
            }

            return true;
        }
        
        private object[] GetIoCParameterValues(Type type, MethodBase selectedConstructor, IDictionary<string, object> arguments)
        {
            var parameters = new List<object>();
            foreach (var parameterInfo in selectedConstructor.GetParameters())
            {
                if (arguments != null && arguments.ContainsKey(parameterInfo.Name))
                {
                    parameters.Add(arguments[parameterInfo.Name]);
                }
                else if (TryResolveParameter(type, parameterInfo, out var parameterValue))
                {
                    parameters.Add(parameterValue);
                }
            }
            return parameters.ToArray();
        }

        private void CheckCollection()
        {
            if (ServiceCollection == null) throw new ArgumentNullException("Can't access ServiceCollection, since it has not been initialized yet.");
            
            if (!_options.InvalidateServiceProviderAtRegistrationsAfterBuild) return;
            if (ServiceProvider == null) return;
            
            _iocServices.InvalidateServiceProvider();
        }
        
        private void CheckProvider()
        {
            if (!_options.BuildServiceProviderAtFirstResolve)
            {
                if (ServiceProvider == null) throw new ArgumentNullException("Can't access ServiceProvider, since it has not been initialized yet.");                
                return;
            }
            
            if (ServiceProvider != null) return;
            
            _iocServices.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = _options.TryToDetectDynamicCircularReferences || _options.TryToDetectSingletonCircularReferences
            });
        }
    }
}
