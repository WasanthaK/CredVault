namespace CredVault.Mobile;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
		
		// Check if user is already authenticated on startup
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			await CheckAuthenticationStateAsync();
		});
		
		return window;
	}

	private async Task CheckAuthenticationStateAsync()
	{
		try
		{
			// Check if access token exists in SecureStorage
			var accessToken = await SecureStorage.GetAsync("access_token");
			
			if (!string.IsNullOrEmpty(accessToken))
			{
				// User is logged in, navigate to dashboard
				await Shell.Current.GoToAsync("//dashboard");
			}
			else
			{
				// User is not logged in, navigate to login page
				await Shell.Current.GoToAsync("//login");
			}
		}
		catch
		{
			// If there's any error, default to login page
			await Shell.Current.GoToAsync("//login");
		}
	}
}
