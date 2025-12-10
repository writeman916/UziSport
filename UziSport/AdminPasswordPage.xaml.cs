using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using UziSport.Services;

namespace UziSport
{
    public partial class AdminPasswordPage : ContentPage
    {
        private readonly string _targetRoute;

        public AdminPasswordPage(string targetRoute)
        {
            InitializeComponent();
            _targetRoute = targetRoute;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            bool ok = await PasswordPopup.ShowAsync();

            if (ok)
            {
                AdminAuthService.SetAuthorized(true);

                await Shell.Current.GoToAsync(_targetRoute, animate: true);
            }

            await Navigation.PopModalAsync();
        }
    }
}
