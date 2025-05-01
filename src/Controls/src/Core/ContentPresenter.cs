#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ContentPresenter.xml" path="Type[@FullName='Microsoft.Maui.Controls.ContentPresenter']/Docs/*" />
	public class ContentPresenter : View, ILayout, ILayoutController, IPaddingElement, IView, IVisualTreeElement, IInputTransparentContainerElement, IContentView
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ContentPresenter.xml" path="//Member[@MemberName='ContentProperty']/Docs/*" />
		public static BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View),
			typeof(ContentPresenter), null, propertyChanged: OnContentChanged);

		[Obsolete("Use SizeChanged.")]
		public event EventHandler LayoutChanged;

		/// <include file="../../docs/Microsoft.Maui.Controls/ContentPresenter.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ContentPresenter()
		{
			this.SetBinding(
				ContentProperty,
				static (IContentView view) => view.Content,
				source: RelativeBindingSource.TemplatedParent,
				converter: new ContentConverter(),
				converterParameter: this);
		}

		/// <summary>
		/// Gets or sets a value that controls whether child elements
		/// inherit the input transparency of this layout when the tranparency is <see langword="true"/>.
		/// </summary>
		/// <value>
		/// <see langword="true" /> to cause child elements to inherit the input transparency of this layout,
		/// when this layout's <see cref="VisualElement.InputTransparent" /> property is <see langword="true" />.
		/// <see langword="false" /> to cause child elements to ignore the input tranparency of this layout.
		/// </value>
		public bool CascadeInputTransparent
		{
			get => (bool)GetValue(InputTransparentContainerElement.CascadeInputTransparentProperty);
			set => SetValue(InputTransparentContainerElement.CascadeInputTransparentProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ContentPresenter.xml" path="//Member[@MemberName='Content']/Docs/*" />
		public View Content
		{
			get { return (View)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		object IContentView.Content => Content;
		IView IContentView.PresentedContent => Content;

		IReadOnlyList<Element> ILayoutController.Children => LogicalChildrenInternal;

		/// <summary>
		/// Gets or sets the inner padding of the layout.
		/// The default value is a <see cref="Thickness"/> with all values set to 0.
		/// </summary>
		/// <remarks>The padding is the space between the bounds of a layout and the bounding region into which its children should be arranged into.</remarks>
		public Thickness Padding
		{
			get => (Thickness)GetValue(PaddingElement.PaddingProperty);
			set => SetValue(PaddingElement.PaddingProperty, value);
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator() => default(Thickness);

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue) => InvalidateMeasure();

		internal virtual void Clear()
		{
			Content = null;
		}

		internal override void ComputeConstraintForView(View view)
		{
			bool isFixedHorizontally = (Constraint & LayoutConstraint.HorizontallyFixed) != 0;
			bool isFixedVertically = (Constraint & LayoutConstraint.VerticallyFixed) != 0;

			var result = LayoutConstraint.None;
			if (isFixedVertically && view.VerticalOptions.Alignment == LayoutAlignment.Fill)
				result |= LayoutConstraint.VerticallyFixed;
			if (isFixedHorizontally && view.HorizontalOptions.Alignment == LayoutAlignment.Fill)
				result |= LayoutConstraint.HorizontallyFixed;
			view.ComputedConstraint = result;
		}

		internal override void SetChildInheritedBindingContext(Element child, object context)
		{
			// We never want to use the standard inheritance mechanism, we will get this set by our parent
		}

		static async void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = (ContentPresenter)bindable;

			var oldView = (View)oldValue;
			var newView = (View)newValue;
			if (oldView != null)
			{
				self.RemoveLogicalChild(oldView);
				oldView.ParentOverride = null;
			}

			if (newView != null)
			{
				self.AddLogicalChild(newView);
				newView.ParentOverride = await TemplateUtilities.FindTemplatedParentAsync((Element)bindable);
			}
		}

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			return this.ComputeDesiredSize(widthConstraint, heightConstraint);
		}

		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return this.MeasureContent(widthConstraint, heightConstraint);
		}

		protected override Size ArrangeOverride(Rect bounds)
		{
			Frame = this.ComputeFrame(bounds);
			Handler?.PlatformArrange(Frame);
			return Frame.Size;
		}

		Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds)
		{
			this.ArrangeContent(bounds);
			return bounds.Size;
		}

		Size IContentView.CrossPlatformMeasure(double widthConstraint, double heightConstraint) => ((ICrossPlatformLayout)this).CrossPlatformMeasure(widthConstraint, heightConstraint);

		Size IContentView.CrossPlatformArrange(Rect bounds) =>
			((ICrossPlatformLayout)this).CrossPlatformArrange(bounds);
	}
}