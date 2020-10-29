﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MvvmCross.IoC;
using MvvmCross.Plugin;

namespace MvvmCross.ViewModels
{
    public interface IMvxApplication : IMvxViewModelLocatorCollection
    {
        IMvxIocServices IocServices { get; set; }
        
        void LoadPlugins(IMvxPluginManager pluginManager);

        void Initialize();

        Task Startup();

        void Reset();
    }

    public interface IMvxApplication<THint> : IMvxApplication
    {
        THint Startup(THint hint);
    }
}
