// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Extensions.DependencyInjection;
using MvvmCross.Base;
using MvvmCross.Core;
using MvvmCross.IoC;
using MvvmCross.ViewModels;
using MvvmCross.Views;
using Xunit;
// ReSharper disable VirtualMemberCallInConstructor

namespace MvvmCross.UnitTest.Base
{
    [Collection("MvxTest")]
    public class MvxIocPropertyInjectionTest
    {
        private Setup _setup;
        private IMvxIoCProvider _iocProvider;

        public MvxIocPropertyInjectionTest()
        {
            _setup = new Setup();
            _setup.InitializePrimary();
            _iocProvider = CreateIoCProvider();
        }

        protected virtual IMvxIoCProvider CreateIoCProvider(IMvxIocOptions options = null)
        {
            return MvxIoCProvider.Initialize(_setup);
        }
        
        #region Mocked Setup

        private class Setup : MvxSetup
        {
            protected override IMvxApplication CreateApp() => throw new NotImplementedException();
            protected override IMvxViewsContainer CreateViewsContainer() => throw new NotImplementedException();
            protected override IMvxViewDispatcher CreateViewDispatcher() => throw new NotImplementedException();
            protected override IMvxNameMapping CreateViewToViewModelNaming() => throw new NotImplementedException();

            public override void InitializePrimary()
            {
                var serviceCollection = InitializeIoC();
                ServiceCollection = serviceCollection;
            }

            public override IServiceProvider InitializeSecondary()
            {
                ServiceProvider = BuildServiceProvider(new ServiceProviderOptions());
                return ServiceProvider;
            }
        }
        
        #endregion
        
        public interface IA
        {
        }

        public interface IB 
        {
        }

        public interface IC
        {
        }

        public class A : IA
        {
            public A()
            {
            }

            [MvxInject]
            public IB B { get; set; }

            public IC C { get; set; }

            public B BNever { get; set; }

            [MvxInject]
            public C CNever { get; set; }
        }

        public class B : IB 
        {
        }

        public class C : IC
        {
        }

        [Fact]
        public void TryResolve_WithNoInjection_NothingGetsInjected()
        {
            MvxSingleton.ClearAllSingletons();
            //var instance = MvxIoCProvider.Initialize();
            var instance = CreateIoCProvider();

            Mvx.IoCProvider.RegisterType<IA, A>();
            Mvx.IoCProvider.RegisterType<IB, B>();
            Mvx.IoCProvider.RegisterType<IC, C>();

            IA a;
            var result = Mvx.IoCProvider.TryResolve(out a);
            Assert.True(result);
            Assert.NotNull(a);
            Assert.IsType<A>(a);
            var castA = (A)a;
            Assert.Null(castA.B);
            Assert.Null(castA.C);
            Assert.Null(castA.BNever);
            Assert.Null(castA.CNever);
        }

        [Fact]
        public void TryResolve_WithAttrInjection_AttrMarkedProperiesGetInjected()
        {
            MvxSingleton.ClearAllSingletons();
            var options = new MvxIocOptions
            {
                PropertyInjectorOptions = new MvxPropertyInjectorOptions()
                {
                    InjectIntoProperties = MvxPropertyInjection.MvxInjectInterfaceProperties
                }
            };
            //var instance = MvxIoCProvider.Initialize(options);
            var instance = CreateIoCProvider();

            Mvx.IoCProvider.RegisterType<IA, A>();
            Mvx.IoCProvider.RegisterType<IB, B>();
            Mvx.IoCProvider.RegisterType<IC, C>();

            IA a;
            var result = Mvx.IoCProvider.TryResolve(out a);
            Assert.True(result);
            Assert.NotNull(a);
            Assert.IsType<A>(a);
            var castA = (A)a;
            Assert.NotNull(castA.B);
            Assert.IsType<B>(castA.B);
            Assert.Null(castA.C);
            Assert.Null(castA.BNever);
            Assert.Null(castA.CNever);
        }

        [Fact]
        public void TryResolve_WithFullInjection_AllInterfaceProperiesGetInjected()
        {
            MvxSingleton.ClearAllSingletons();
            var options = new MvxIocOptions
            {
                PropertyInjectorOptions = new MvxPropertyInjectorOptions()
                {
                    InjectIntoProperties = MvxPropertyInjection.AllInterfaceProperties
                }
            };
            //var instance = MvxIoCProvider.Initialize(options);
            var instance = CreateIoCProvider();

            Mvx.IoCProvider.RegisterType<IA, A>();
            Mvx.IoCProvider.RegisterType<IB, B>();
            Mvx.IoCProvider.RegisterType<IC, C>();

            IA a;
            var result = Mvx.IoCProvider.TryResolve(out a);
            Assert.True(result);
            Assert.NotNull(a);
            Assert.IsType<A>(a);
            var castA = (A)a;
            Assert.NotNull(castA.B);
            Assert.IsType<B>(castA.B);
            Assert.NotNull(castA.C);
            Assert.IsType<C>(castA.C);
            Assert.Null(castA.BNever);
            Assert.Null(castA.CNever);
        }
    }
}
