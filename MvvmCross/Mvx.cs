// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using MvvmCross.Base;
using MvvmCross.IoC;

namespace MvvmCross
{
    public static class Mvx
    {
        /// <summary>
        /// Returns a singleton instance of the default IoC Provider. If possible use dependency injection instead.
        /// </summary>
        [Obsolete("Use Mvx.ServiceProvider instead. If possible use dependency injection instead.")]
        public static IMvxIoCProvider IoCProvider => MvxSingleton<IMvxIoCProvider>.Instance;
        
        /// <summary>
        /// Returns a singleton instance of the IServiceCollection to register services.
        /// This gets set by the MvxSetup.
        /// If possible use dependency injection instead.
        /// </summary>
        public static IServiceCollection ServiceCollection
        {
            get;
            internal set;
        }
        
        /// <summary>
        /// Returns a singleton instance of the IServiceProvider to resolve services.
        /// This gets set by the MvxSetup.
        /// If possible use dependency injection instead.
        /// </summary>
        public static IServiceProvider ServiceProvider {
            get;
            internal set;
        }
    }
}
