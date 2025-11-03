using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using CredVault.Mobile.Services;
using CredVault.Mobile.ViewModels;
using Refit;
using ZXing.Net.Maui.Controls;

namespace CredVault.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
#pragma warning disable CA1416 // Validate platform compatibility
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseBarcodeReader()
#pragma warning restore CA1416
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Configure HTTP clients with Refit for all API services
		ConfigureApiClients(builder.Services);

		// Register application services
		ConfigureServices(builder.Services);

		// Register ViewModels
		ConfigureViewModels(builder.Services);

		// Register Pages
		ConfigurePages(builder.Services);

#if DEBUG
		builder.Logging.AddDebug();
		builder.Logging.SetMinimumLevel(LogLevel.Debug);
#else
		builder.Logging.SetMinimumLevel(LogLevel.Information);
#endif

		return builder.Build();
	}

	private static void ConfigureApiClients(IServiceCollection services)
	{
		// Register AuthHeaderHandler for automatic Bearer token injection
		services.AddTransient<AuthHeaderHandler>();

		// Wallet API Client (Port 7015) - with auth handler
		services.AddRefitClient<IWalletApiClient>()
			.ConfigureHttpClient(c =>
			{
				c.BaseAddress = new Uri(ApiConfiguration.GetWalletBaseUrl());
				c.Timeout = TimeSpan.FromSeconds(30);
			})
			.AddHttpMessageHandler<AuthHeaderHandler>();

		// Identity API Client (Port 7001) - with auth handler
		services.AddRefitClient<IIdentityApiClient>()
			.ConfigureHttpClient(c =>
			{
				c.BaseAddress = new Uri(ApiConfiguration.GetIdentityBaseUrl());
				c.Timeout = TimeSpan.FromSeconds(30);
			})
			.AddHttpMessageHandler<AuthHeaderHandler>();

		// Consent API Client (Port 7002) - with auth handler
		services.AddRefitClient<IConsentApiClient>()
			.ConfigureHttpClient(c =>
			{
				c.BaseAddress = new Uri(ApiConfiguration.GetConsentBaseUrl());
				c.Timeout = TimeSpan.FromSeconds(30);
			})
			.AddHttpMessageHandler<AuthHeaderHandler>();

		// Payments API Client (Port 7004) - with auth handler
		services.AddRefitClient<IPaymentsApiClient>()
			.ConfigureHttpClient(c =>
			{
				c.BaseAddress = new Uri(ApiConfiguration.GetPaymentsBaseUrl());
				c.Timeout = TimeSpan.FromSeconds(30);
			})
			.AddHttpMessageHandler<AuthHeaderHandler>();
	}

	private static void ConfigureServices(IServiceCollection services)
	{
		// Register service wrappers as singletons
		services.AddSingleton<WalletService>();
		services.AddSingleton<IdentityService>();
		services.AddSingleton<ConsentService>();
		services.AddSingleton<PaymentsService>();
		services.AddSingleton<ISecurityService, SecurityService>();
		services.AddSingleton<AuthenticationFlowService>();
		services.AddSingleton<AppModeService>();

		// Register navigation service
		services.AddSingleton<INavigationService, NavigationService>();

		// Register platform services
		services.AddSingleton<ISecureStorage>(SecureStorage.Default);
		services.AddSingleton(Connectivity.Current);
		services.AddSingleton(Preferences.Default);
	}

	private static void ConfigureViewModels(IServiceCollection services)
	{
		// Register ViewModels as transient (new instance per request)
		services.AddTransient<BaseViewModel>();
		services.AddTransient<LoginViewModel>();
		services.AddTransient<RegisterViewModel>();
		services.AddTransient<DashboardViewModel>();
		services.AddTransient<CredentialsListViewModel>();
		services.AddTransient<CredentialDetailsViewModel>();
		services.AddTransient<AddCredentialViewModel>();
		services.AddTransient<QrScannerViewModel>();
		services.AddTransient<PresentationViewModel>();
		services.AddTransient<ProfileViewModel>();
		services.AddTransient<BackupViewModel>();
		services.AddTransient<ActivityLogViewModel>();
		services.AddTransient<SettingsViewModel>();
		services.AddTransient<SecuritySettingsViewModel>();
		services.AddTransient<VerifierViewModel>();
		services.AddTransient<ManualRequestViewModel>();
		services.AddTransient<PinViewModel>();
	}

	private static void ConfigurePages(IServiceCollection services)
	{
		// Register Pages as transient
		services.AddTransient<Views.TestPage>();
		services.AddTransient<Views.LoginPage>();
		services.AddTransient<Views.RegisterPage>();
		services.AddTransient<Views.DashboardPage>();
		services.AddTransient<Views.CredentialsListPage>();
		services.AddTransient<Views.CredentialDetailsPage>();
		services.AddTransient<Views.SelectCredentialTypePage>();
		services.AddTransient<Views.AuthenticateCredentialPage>();
		services.AddTransient<Views.ConsentReviewPage>();
		services.AddTransient<Views.CredentialConfirmationPage>();
		services.AddTransient<Views.QrScannerPage>();
		services.AddTransient<Views.PresentationRequestPage>();
		services.AddTransient<Views.PresentationResultPage>();
		services.AddTransient<Views.SelectiveDisclosurePage>();
		services.AddTransient<Views.ProfilePage>();
		services.AddTransient<Views.BackupPage>();
		services.AddTransient<Views.ActivityLogPage>();
		services.AddTransient<Views.SettingsPage>();
		services.AddTransient<Views.SecuritySettingsPage>();
		services.AddTransient<Views.VerifierHomePage>();
		services.AddTransient<Views.VerificationResultPage>();
		services.AddTransient<Views.ManualRequestPage>();
		services.AddTransient<Views.PinPage>();
	}
}
