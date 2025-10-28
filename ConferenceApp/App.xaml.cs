using ConferenceApp.Services;

namespace ConferenceApp;

public partial class App : Application
{
	public App(IFeatureFlagService flagService)
	{
		InitializeComponent();
		
		// Initialize feature flags on app start
		_ = Task.Run(async () => await flagService.InitializeAsync());
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}