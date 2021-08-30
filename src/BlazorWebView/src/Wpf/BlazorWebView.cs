// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.WebView2;
using Microsoft.Extensions.FileProviders;
using WebView2Control = Microsoft.Web.WebView2.Wpf.WebView2;

namespace Microsoft.AspNetCore.Components.WebView.Wpf
{
    /// <summary>
    /// A Windows Presentation Foundation (WPF) control for hosting Blazor web components locally in Windows desktop applications.
    /// </summary>
    public class BlazorWebView : Control, IAsyncDisposable
    {
        #region Dependency property definitions
        /// <summary>
        /// The backing store for the <see cref="HostPage"/> property.
        /// </summary>
        public static readonly DependencyProperty HostPageProperty = DependencyProperty.Register(
            name: nameof(HostPage),
            propertyType: typeof(string),
            ownerType: typeof(BlazorWebView),
            typeMetadata: new PropertyMetadata(OnHostPagePropertyChanged));

        /// <summary>
        /// The backing store for the <see cref="RootComponent"/> property.
        /// </summary>
        public static readonly DependencyProperty RootComponentsProperty = DependencyProperty.Register(
            name: nameof(RootComponents),
            propertyType: typeof(ObservableCollection<RootComponent>),
            ownerType: typeof(BlazorWebView));

        /// <summary>
        /// The backing store for the <see cref="Services"/> property.
        /// </summary>
        public static readonly DependencyProperty ServicesProperty = DependencyProperty.Register(
            name: nameof(Services),
            propertyType: typeof(IServiceProvider),
            ownerType: typeof(BlazorWebView),
            typeMetadata: new PropertyMetadata(OnServicesPropertyChanged));
        #endregion

        private const string webViewTemplateChildName = "WebView";
        private WebView2Control _webview;
        private WebView2WebViewManager _webviewManager;
        private bool _isDisposed;

        /// <summary>
        /// Creates a new instance of <see cref="BlazorWebView"/>.
        /// </summary>
        public BlazorWebView()
        {
            SetValue(RootComponentsProperty, new ObservableCollection<RootComponent>());
            RootComponents.CollectionChanged += HandleRootComponentsCollectionChanged;

            Template = new ControlTemplate
            {
                VisualTree = new FrameworkElementFactory(typeof(WebView2Control), webViewTemplateChildName)
            };
        }

		/// <summary>
		/// Returns the inner <see cref="WebView2Control"/> used by this control.
		/// </summary>
		/// <remarks>
		/// Directly using some functionality of the inner web view can cause unexpected results because its behavior
		/// is controlled by the <see cref="BlazorWebView"/> that is hosting it.
		/// </remarks>
		[Browsable(false)]
		public WebView2Control WebView => _webview;

        /// <summary>
        /// Path to the host page within the application's static files. For example, <code>wwwroot\index.html</code>.
        /// This property must be set to a valid value for the Blazor components to start.
        /// </summary>
        public string HostPage
        {
            get => (string)GetValue(HostPageProperty);
            set => SetValue(HostPageProperty, value);
        }

        /// <summary>
        /// A collection of <see cref="RootComponent"/> instances that specify the Blazor <see cref="IComponent"/> types
        /// to be used directly in the specified <see cref="HostPage"/>.
        /// </summary>
        public ObservableCollection<RootComponent> RootComponents =>
            (ObservableCollection<RootComponent>)GetValue(RootComponentsProperty);

        /// <summary>
        /// Gets or sets an <see cref="IServiceProvider"/> containing services to be used by this control and also by application code.
        /// This property must be set to a valid value for the Blazor components to start.
        /// </summary>
        public IServiceProvider Services
        {
            get => (IServiceProvider)GetValue(ServicesProperty);
            set => SetValue(ServicesProperty, value);
        }

        private static void OnServicesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((BlazorWebView)d).OnServicesPropertyChanged(e);

        private void OnServicesPropertyChanged(DependencyPropertyChangedEventArgs e) => StartWebViewCoreIfPossible();

        private static void OnHostPagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((BlazorWebView)d).OnHostPagePropertyChanged(e);

        private void OnHostPagePropertyChanged(DependencyPropertyChangedEventArgs e) => StartWebViewCoreIfPossible();

        private bool RequiredStartupPropertiesSet =>
            _webview != null &&
            HostPage != null &&
            Services != null;

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            CheckDisposed();

            // Called when the control is created after its child control (the WebView2) is created from the Template property
            base.OnApplyTemplate();

            if (_webview == null)
            {
                _webview = (WebView2Control)GetTemplateChild(webViewTemplateChildName);
                StartWebViewCoreIfPossible();
            }
        }

        /// <inheritdoc />
        protected override void OnInitialized(EventArgs e)
        {
            // Called when BeginInit/EndInit are used, such as when creating the control from XAML
            base.OnInitialized(e);
            StartWebViewCoreIfPossible();
        }

        private void StartWebViewCoreIfPossible()
        {
            CheckDisposed();

            if (!RequiredStartupPropertiesSet || _webviewManager != null)
            {
                return;
            }

            // We assume the host page is always in the root of the content directory, because it's
            // unclear there's any other use case. We can add more options later if so.
            var contentRootDir = Path.GetDirectoryName(Path.GetFullPath(HostPage));
            var hostPageRelativePath = Path.GetRelativePath(contentRootDir, HostPage);
            var fileProvider = new PhysicalFileProvider(contentRootDir);

			var jsComponents = new JSComponentConfigurationStore();
			_webviewManager = new WebView2WebViewManager(new WpfWebView2Wrapper(_webview), Services, WpfDispatcher.Instance, fileProvider, jsComponents, hostPageRelativePath);
            foreach (var rootComponent in RootComponents)
            {
                // Since the page isn't loaded yet, this will always complete synchronously
                _ = rootComponent.AddToWebViewManagerAsync(_webviewManager);
            }
            _webviewManager.Navigate("/");
        }

        private void HandleRootComponentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            CheckDisposed();

            // If we haven't initialized yet, this is a no-op
            if (_webviewManager != null)
            {
                // Dispatch because this is going to be async, and we want to catch any errors
                WpfDispatcher.Instance.InvokeAsync(async () =>
                {
                    var newItems = eventArgs.NewItems.Cast<RootComponent>();
                    var oldItems = eventArgs.OldItems.Cast<RootComponent>();

                    foreach (var item in newItems.Except(oldItems))
                    {
                        await item.AddToWebViewManagerAsync(_webviewManager);
                    }

                    foreach (var item in oldItems.Except(newItems))
                    {
                        await item.RemoveFromWebViewManagerAsync(_webviewManager);
                    }
                });
            }
        }

        private void CheckDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
			// Dispose this component's contents that user-written disposal logic and Blazor disposal logic will complete
			// first. Then dispose the WebView2 control. This order is critical because once the WebView2 is disposed it
			// will prevent and Blazor code from working because it requires the WebView to exist.
			if (_webviewManager != null)
            {
                await _webviewManager.DisposeAsync()
                    .ConfigureAwait(false);
                _webviewManager = null;
            }

            _webview?.Dispose();
            _webview = null;
        }

        public async ValueTask DisposeAsync()
        {
			if (_isDisposed)
			{
				return;
			}
			_isDisposed = true;

			// Perform async cleanup.
			await DisposeAsyncCore();

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            // Suppress finalization.
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize	
        }
    }
}
