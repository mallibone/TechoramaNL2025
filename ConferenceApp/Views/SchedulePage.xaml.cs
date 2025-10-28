using ConferenceApp.ViewModels;
using ConferenceApp.Models;

namespace ConferenceApp.Views;

public partial class SchedulePage : ContentPage
{
	private ScheduleViewModel? _viewModel;

	public SchedulePage(ScheduleViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		_viewModel = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		
		if (_viewModel != null)
		{
			await _viewModel.LoadDataCommand.ExecuteAsync(null);
		}
	}

	private void OnDaySelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is Day day && _viewModel != null)
		{
			_viewModel.SelectedDayIndex = day.Index;
		}
	}

	private void OnTrackFilterChanged(object? sender, EventArgs e)
	{
		if (sender is Picker picker && _viewModel != null && picker.SelectedItem is Track track)
		{
			_viewModel.SelectedTrackId = track.Id;
		}
	}
}
