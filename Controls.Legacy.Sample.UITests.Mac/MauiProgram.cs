﻿using Maui.Controls.Legacy.Sample.Mac.Services;
using Maui.Controls.Legacy.Sample.Services;

namespace Maui.Controls.Legacy.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder
			.UseSharedMauiApp();

		builder.Services.AddTransient<IPlatformSpecificService, MacCatalystService>();

		return builder.Build();
	}
}
