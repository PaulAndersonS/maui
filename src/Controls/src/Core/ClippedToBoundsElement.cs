namespace Microsoft.Maui.Controls;

static class ClippedToBoundsElement
{
	public static readonly BindableProperty IsClippedToBoundsProperty =
			BindableProperty.Create("IsClippedToBounds", typeof(bool), typeof(Layout), false,
			propertyChanged: IsClippedToBoundsPropertyChanged);
}
