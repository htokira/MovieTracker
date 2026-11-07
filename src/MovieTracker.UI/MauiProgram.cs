using Microsoft.Extensions.Logging;
using MovieTracker.UI.Services;
using MovieTracker.UI.ViewModels;
using MovieTracker.UI.Views;

namespace MovieTracker.UI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		#if ANDROID
				var apiBaseUrl = "http://10.0.2.2:7226/";
		#else
				var apiBaseUrl = "https://localhost:7226/";
		#endif

		var builder = MauiApp.CreateBuilder();
		builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<ApiService>();
		builder.Services.AddTransient<MainViewModel>();
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<MovieDetailsViewModel>();
		builder.Services.AddTransient<MovieDetailsPage>();
		builder.Services.AddSingleton<AppShell>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
