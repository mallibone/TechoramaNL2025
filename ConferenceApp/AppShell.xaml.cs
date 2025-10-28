using ConferenceApp.Views;

namespace ConferenceApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Register routes for navigation
		Routing.RegisterRoute("session", typeof(SessionDetailPage));
		Routing.RegisterRoute("feedback", typeof(FeedbackPage));
		
#if DEBUG
		Routing.RegisterRoute("flags", typeof(FlagsDebugPage));
		
		// Add debug menu item
		var debugTab = new Tab
		{
			Title = "Debug",
			Icon = "settings.png",
			IsEnabled = true,
			IsVisible = true,
			Items =
			{
				new ShellContent
				{
					Title = "Feature Flags",
					ContentTemplate = new DataTemplate(typeof(FlagsDebugPage))
				}
			}
		};
		Items.OfType<TabBar>().FirstOrDefault()?.Items.Add(debugTab);
#endif
	}
}
