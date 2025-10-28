using ConferenceApp.ViewModels;

namespace ConferenceApp.Views;

public partial class SessionDetailPage : ContentPage
{
	public SessionDetailPage(SessionDetailViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
