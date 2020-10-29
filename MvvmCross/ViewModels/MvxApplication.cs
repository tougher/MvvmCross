﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MvvmCross.IoC;
using MvvmCross.Logging;
using MvvmCross.Plugin;

namespace MvvmCross.ViewModels
{
    public abstract class MvxApplication : IMvxApplication
    {
        private IMvxViewModelLocator _defaultLocator;

        private IMvxViewModelLocator DefaultLocator
        {
            get
            {
                _defaultLocator = _defaultLocator ?? CreateDefaultViewModelLocator();
                return _defaultLocator;
            }
        }
        
        public IMvxIocServices IocServices { get; set; }

        protected virtual IMvxViewModelLocator CreateDefaultViewModelLocator()
        {
            return new MvxDefaultViewModelLocator();
        }

        public virtual void LoadPlugins(IMvxPluginManager pluginManager)
        {
            // do nothing
        }

        /// <summary>
        /// Any initialization steps that can be done in the background
        /// </summary>
        public virtual void Initialize()
        {
            // do nothing
        }

        /// <summary>
        /// Any initialization steps that need to be done on the UI thread
        /// </summary>
        public virtual Task Startup()
        {
            MvxLog.Instance.Trace("AppStart: Application Startup - On UI thread");
            return Task.CompletedTask;
        }

        /// <summary>
        /// If the application is restarted (eg primary activity on Android 
        /// can be restarted) this method will be called before Startup
        /// is called again
        /// </summary>
        public virtual void Reset()
        {
            // do nothing
        }

        public IMvxViewModelLocator FindViewModelLocator(MvxViewModelRequest request)
        {
            return DefaultLocator;
        }

        protected void RegisterCustomAppStart<TMvxAppStart>()
            where TMvxAppStart : class, IMvxAppStart
        {
            var appStart = ActivatorUtilities.CreateInstance<TMvxAppStart>(IocServices.ServiceProvider);
            IocServices.ServiceCollection.AddSingleton(appStart);
        }

        protected void RegisterAppStart<TViewModel>()
            where TViewModel : IMvxViewModel
        {
            var appStart = ActivatorUtilities.CreateInstance<MvxAppStart<TViewModel>>(IocServices.ServiceProvider);
            IocServices.ServiceCollection.AddSingleton(appStart);
        }

        protected void RegisterAppStart(IMvxAppStart appStart)
        {
            IocServices.ServiceCollection.AddSingleton(appStart);
        }

        protected virtual void RegisterAppStart<TViewModel, TParameter>()
          where TViewModel : IMvxViewModel<TParameter> where TParameter : class
        {
            var appStart = ActivatorUtilities.CreateInstance<MvxAppStart<TViewModel, TParameter>>(IocServices.ServiceProvider);
            IocServices.ServiceCollection.AddSingleton(appStart);
        }

        protected IEnumerable<Type> CreatableTypes()
        {
            return CreatableTypes(GetType().GetTypeInfo().Assembly);
        }

        protected IEnumerable<Type> CreatableTypes(Assembly assembly)
        {
            return assembly.CreatableTypes();
        }
    }

    public class MvxApplication<TParameter> : MvxApplication, IMvxApplication<TParameter>
    {
        public virtual TParameter Startup(TParameter parameter)
        {
            // do nothing, so just return the original hint
            return parameter;
        }
    }
}
