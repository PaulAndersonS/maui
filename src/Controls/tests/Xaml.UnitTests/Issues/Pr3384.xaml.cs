using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime|XamlInflator.XamlC, true)]
public partial class Pr3384 : ContentPage
{
	public Pr3384() => InitializeComponent();

	[TestFixture]
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.iOS));
		}

		[TearDown]
		public void TearDown()
		{
			DispatcherProvider.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Test] public void RecyclingStrategyIsHandled([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{ 
				var result = MockSourceGenerator.CreateMauiCompilation().RunMauiSourceGenerator(typeof(Pr3384));
				Assert.That(result.Diagnostics.Any());
			}
			
			var p = new Pr3384(inflator);
			Assert.AreEqual(ListViewCachingStrategy.RecycleElement, p.listView.CachingStrategy);
		}
	}
}