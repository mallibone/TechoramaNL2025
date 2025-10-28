using ConferenceApp.ViewModels;

namespace ConferenceApp.Views;

public partial class FlagsDebugPage : ContentPage
{
	private readonly FlagsDebugViewModel _viewModel;

	public FlagsDebugPage(FlagsDebugViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = viewModel;
	}

	private void OnSessionFeedbackToggled(object sender, ToggledEventArgs e)
	{
		// Execute toggle command - it will update the property via FlagsChanged event
		if (_viewModel.ToggleSessionFeedbackCommand.CanExecute(null))
		{
			_viewModel.ToggleSessionFeedbackCommand.Execute(null);
		}
	}
}
