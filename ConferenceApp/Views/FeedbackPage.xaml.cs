using ConferenceApp.ViewModels;

namespace ConferenceApp.Views;

public partial class FeedbackPage : ContentPage
{
	public FeedbackPage(FeedbackViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
