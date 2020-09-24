// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Logging;
using MvvmCross.Presenters.Attributes;
using MvvmCross.Presenters.Hints;
using MvvmCross.ViewModels;
using MvvmCross.Views;

namespace MvvmCross.Presenters
{
#nullable enable
    public abstract class MvxAttributeViewPresenter : MvxViewPresenter, IMvxAttributeViewPresenter
    {
        private IMvxViewModelTypeFinder? _viewModelTypeFinder;
        public virtual IMvxViewModelTypeFinder ViewModelTypeFinder
        {
            get => _viewModelTypeFinder ??= Mvx.IoCProvider.Resolve<IMvxViewModelTypeFinder>();
        }

        private IMvxViewsContainer? _viewsContainer;
        public virtual IMvxViewsContainer ViewsContainer
        {
            get => _viewsContainer ??= Mvx.IoCProvider.Resolve<IMvxViewsContainer>();
        }

        private IDictionary<Type, MvxPresentationAttributeAction>? _attributeTypesActionsDictionary;
        public virtual IDictionary<Type, MvxPresentationAttributeAction> AttributeTypesToActionsDictionary
        {
            get
            {
                if (_attributeTypesActionsDictionary == null)
                {
                    _attributeTypesActionsDictionary = new Dictionary<Type, MvxPresentationAttributeAction>();
                    RegisterAttributeTypes(_attributeTypesActionsDictionary);
                }
                return _attributeTypesActionsDictionary;
            }
        }

        public abstract void RegisterAttributeTypes(IDictionary<Type, MvxPresentationAttributeAction> attributeTypes);

        public abstract MvxBasePresentationAttribute CreatePresentationAttribute(Type viewModelType, Type viewType);

        public virtual MvxBasePresentationAttribute? GetOverridePresentationAttribute(MvxViewModelRequest request, Type viewType)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            if (viewType?.GetInterfaces().Contains(typeof(IMvxOverridePresentationAttribute)) ?? false)
            {
                var viewInstance = Activator.CreateInstance(viewType) as IMvxOverridePresentationAttribute;
                try
                {
                    var presentationAttribute = viewInstance?.PresentationAttribute(request);
                    if (presentationAttribute == null)
                    {
                        MvxLog.Instance.Warn("Override PresentationAttribute null. Falling back to existing attribute.");
                    }
                    else
                    {
                        if (presentationAttribute.ViewType == null)
                        {
                            presentationAttribute.ViewType = viewType;
                        }

                        if (presentationAttribute.ViewModelType == null)
                        {
                            presentationAttribute.ViewModelType = request.ViewModelType;
                        }

                        return presentationAttribute;
                    }
                }
                finally
                {
                    (viewInstance as IDisposable)?.Dispose();
                }
            }

            return null;
        }

        public virtual MvxBasePresentationAttribute GetPresentationAttribute(MvxViewModelRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var viewType = ViewsContainer.GetViewType(request.ViewModelType);

            var overrideAttribute = GetOverridePresentationAttribute(request, viewType);
            if (overrideAttribute != null)
                return overrideAttribute;

            if (viewType
                .GetCustomAttributes(typeof(MvxBasePresentationAttribute), true)
                .FirstOrDefault() is MvxBasePresentationAttribute attribute)
            {
                if (attribute.ViewType == null)
                    attribute.ViewType = viewType;

                if (attribute.ViewModelType == null)
                    attribute.ViewModelType = request.ViewModelType;

                return attribute;
            }

            return CreatePresentationAttribute(request.ViewModelType, viewType);
        }

        protected virtual MvxPresentationAttributeAction GetPresentationAttributeAction(MvxViewModelRequest request, out MvxBasePresentationAttribute attribute)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            attribute = GetPresentationAttribute(request);
            attribute.ViewModelType = request.ViewModelType;
            var attributeType = attribute.GetType();

            if (AttributeTypesToActionsDictionary.TryGetValue(
                attributeType,
                out MvxPresentationAttributeAction attributeAction))
            {
                if (attributeAction.ShowAction == null)
                    throw new InvalidOperationException($"attributeAction.ShowAction is null for attribute: {attributeType.Name}");

                if (attributeAction.CloseAction == null)
                    throw new InvalidOperationException($"attributeAction.CloseAction is null for attribute: {attributeType.Name}");

                return attributeAction;
            }

            throw new KeyNotFoundException($"The type {attributeType.Name} is not configured in the presenter dictionary");
        }

        public override async Task<bool> ChangePresentation(MvxPresentationHint hint)
        {
            var handledPresentationChange = await HandlePresentationChange(hint).ConfigureAwait(true);
            if (handledPresentationChange)
                return true;

            if (hint is MvxClosePresentationHint presentationHint)
            {
                var didClose = await Close(presentationHint.ViewModelToClose).ConfigureAwait(true);
                return didClose;
            }

            MvxLog.Instance.Warn("Hint ignored {0}", hint.GetType().Name);
            return false;
        }

        public override Task<bool> Close(IMvxViewModel viewModel)
        {
            return GetPresentationAttributeAction(
                new MvxViewModelInstanceRequest(viewModel), out MvxBasePresentationAttribute attribute)
                    .CloseAction.Invoke(viewModel, attribute);
        }

        public override Task<bool> Show(MvxViewModelRequest request)
        {
            return GetPresentationAttributeAction(
                request, out MvxBasePresentationAttribute attribute)
                    .ShowAction.Invoke(attribute.ViewType, attribute, request);
        }
    }
#nullable restore
}
