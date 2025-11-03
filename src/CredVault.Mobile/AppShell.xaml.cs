using CredVault.Mobile.Views;

namespace CredVault.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		RegisterRoutes();
	}

	private void RegisterRoutes()
	{
		// Register navigation routes for pages
		Routing.RegisterRoute("test", typeof(TestPage));
		Routing.RegisterRoute("login", typeof(LoginPage));
		Routing.RegisterRoute("register", typeof(RegisterPage));
		Routing.RegisterRoute("dashboard", typeof(DashboardPage));
		Routing.RegisterRoute("credentials", typeof(CredentialsListPage));
		Routing.RegisterRoute("credentialdetails", typeof(CredentialDetailsPage));
		Routing.RegisterRoute("addcredential", typeof(SelectCredentialTypePage));
		Routing.RegisterRoute("authenticate", typeof(AuthenticateCredentialPage));
		Routing.RegisterRoute("consent", typeof(ConsentReviewPage));
		Routing.RegisterRoute("confirmation", typeof(CredentialConfirmationPage));
		Routing.RegisterRoute("scanner", typeof(QrScannerPage));
		Routing.RegisterRoute("presentationrequest", typeof(PresentationRequestPage));
		Routing.RegisterRoute("selectivedisclosure", typeof(SelectiveDisclosurePage));
		Routing.RegisterRoute("presentationresult", typeof(PresentationResultPage));
		Routing.RegisterRoute("profile", typeof(ProfilePage));
		Routing.RegisterRoute("backup", typeof(BackupPage));
		Routing.RegisterRoute("activity", typeof(ActivityLogPage));
		Routing.RegisterRoute("settings", typeof(SettingsPage));
		Routing.RegisterRoute("securitysettings", typeof(SecuritySettingsPage));
		Routing.RegisterRoute("verifier", typeof(VerifierHomePage));
		Routing.RegisterRoute("verificationresult", typeof(VerificationResultPage));
		Routing.RegisterRoute("manualrequest", typeof(ManualRequestPage));
		Routing.RegisterRoute("securitypin", typeof(PinPage));
	}
}
