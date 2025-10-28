using Microsoft.Extensions.Logging;
using ConferenceApp.Services;
using ConferenceApp.ViewModels;
using ConferenceApp.Views;
using ConferenceApp.Models;

namespace ConferenceApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Register configuration
		var remoteConfig = new RemoteConfig();
		builder.Services.AddSingleton(remoteConfig);

		// Register HttpClient
		builder.Services.AddSingleton<HttpClient>(sp => new HttpClient
		{
			Timeout = TimeSpan.FromSeconds(10)
		});

		builder.Services.AddSingleton<IRemoteContentService, RemoteContentService>();

		// Register services
		builder.Services.AddSingleton<ILocalStore, LocalJsonStore>();
		builder.Services.AddSingleton<IConferenceRepository, ConferenceRepository>();
		builder.Services.AddSingleton<IFeatureFlagService, FeatureFlagService>();
		builder.Services.AddSingleton<IFeedbackService, FeedbackService>();

		// Register ViewModels
		builder.Services.AddTransient<ScheduleViewModel>();
		builder.Services.AddTransient<SessionDetailViewModel>();
		builder.Services.AddTransient<FavoritesViewModel>();
		builder.Services.AddTransient<FeedbackViewModel>();
		builder.Services.AddTransient<FlagsDebugViewModel>();

		// Register Views
		builder.Services.AddTransient<SchedulePage>();
		builder.Services.AddTransient<SessionDetailPage>();
		builder.Services.AddTransient<FavoritesPage>();
		builder.Services.AddTransient<FeedbackPage>();
		builder.Services.AddTransient<FlagsDebugPage>();

		return builder.Build();
	}
}
