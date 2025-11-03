namespace CredVault.Mobile;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}

	private async void OnDashboardClicked(object sender, EventArgs e)
	{
		try
		{
			await Shell.Current.GoToAsync("dashboard");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Navigation Error", $"Failed to navigate to dashboard: {ex.Message}", "OK");
		}
	}

	private async void OnTestClicked(object sender, EventArgs e)
	{
		try
		{
			await Shell.Current.GoToAsync("test");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Test Error", $"Even the simple test page failed: {ex.Message}\n\nThis means DI container has a fundamental issue.", "OK");
		}
	}

	private async void OnCredentialsClicked(object sender, EventArgs e)
	{
		try
		{
			await Shell.Current.GoToAsync("credentials");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Navigation Error", $"Failed to navigate to credentials: {ex.Message}\n\nStack: {ex.StackTrace}", "OK");
		}
	}

	private async void OnAddCredentialClicked(object sender, EventArgs e)
	{
		try
		{
			await Shell.Current.GoToAsync("addcredential");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Navigation Error", $"Failed to navigate to add credential: {ex.Message}\n\nStack: {ex.StackTrace}", "OK");
		}
	}

	private async void OnScanQrClicked(object sender, EventArgs e)
	{
		try
		{
			await Shell.Current.GoToAsync("scanner");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Navigation Error", $"Failed to navigate to scanner: {ex.Message}\n\nStack: {ex.StackTrace}", "OK");
		}
	}

	private async void OnProfileClicked(object sender, EventArgs e)
	{
		try
		{
			await Shell.Current.GoToAsync("profile");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Navigation Error", $"Failed to navigate to profile: {ex.Message}\n\nStack: {ex.StackTrace}", "OK");
		}
	}

	private async void OnBackupClicked(object sender, EventArgs e)
	{
		try
		{
			await Shell.Current.GoToAsync("backup");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Navigation Error", $"Failed to navigate to backup: {ex.Message}\n\nStack: {ex.StackTrace}", "OK");
		}
	}

	private async void OnActivityClicked(object sender, EventArgs e)
	{
		try
		{
			await Shell.Current.GoToAsync("activity");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Navigation Error", $"Failed to navigate to activity: {ex.Message}\n\nStack: {ex.StackTrace}", "OK");
		}
	}

	private async void OnSettingsClicked(object sender, EventArgs e)
	{
		try
		{
			await Shell.Current.GoToAsync("settings");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Navigation Error", $"Failed to navigate to settings: {ex.Message}\n\nStack: {ex.StackTrace}", "OK");
		}
	}

	private async void OnVerifierClicked(object sender, EventArgs e)
	{
		try
		{
			await Shell.Current.GoToAsync("verifier");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Navigation Error", $"Failed to navigate to verifier: {ex.Message}\n\nStack: {ex.StackTrace}", "OK");
		}
	}
}

