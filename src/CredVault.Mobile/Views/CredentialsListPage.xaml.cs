using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class CredentialsListPage : ContentPage
{
	public CredentialsListPage()
		: this(ServiceHelper.GetRequiredService<CredentialsListViewModel>())
	{
	}

	public CredentialsListPage(CredentialsListViewModel viewModel)
	{
		ArgumentNullException.ThrowIfNull(viewModel);
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is CredentialsListViewModel viewModel)
		{
			await viewModel.InitializeAsync();
		}
	}
}
