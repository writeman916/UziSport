using System;
using Microsoft.Maui.Controls;
using UziSport.Services;

namespace UziSport
{
    public partial class AppShell : Shell
    {
        private bool _isHandlingNavigation;

        public AppShell()
        {
            InitializeComponent();

            Navigating += AppShell_Navigating;
        }

        private async void AppShell_Navigating(object? sender, ShellNavigatingEventArgs e)
        {
            if (_isHandlingNavigation)
                return;

            if (e.Source != ShellNavigationSource.ShellItemChanged &&
                e.Source != ShellNavigationSource.ShellSectionChanged &&
                e.Source != ShellNavigationSource.ShellContentChanged)
            {
                return;
            }

            var targetLocation = e.Target?.Location;
            if (targetLocation == null)
                return;

            var targetRoute = targetLocation.OriginalString ?? string.Empty;

            if (!RequiresAdminPassword(targetRoute))
                return;

            if (AdminAuthService.IsAuthorized)
                return;

            e.Cancel();

            _isHandlingNavigation = true;

            try
            {
                var passwordPage = new AdminPasswordPage(targetRoute);

                await Shell.Current.Navigation.PushModalAsync(passwordPage);
            }
            finally
            {
                _isHandlingNavigation = false;
            }
        }

        private bool RequiresAdminPassword(string route)
        {
            if (route.Contains("admin", StringComparison.OrdinalIgnoreCase))
                return true;

            if (route.Contains("admin-stockin", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
