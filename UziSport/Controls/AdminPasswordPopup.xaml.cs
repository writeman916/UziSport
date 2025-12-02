using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using UziSport.DAL;

namespace UziSport.Controls
{
    public partial class AdminPasswordPopup : ContentView
    {
        private readonly AdminPasswordDAL _adminPasswordDal = new AdminPasswordDAL();

        // Sự kiện để bên ngoài có thể subscribe nếu cần
        public event EventHandler<bool> PasswordValidated;
        public event EventHandler Closed;

        private TaskCompletionSource<bool> _tcs;

        public AdminPasswordPopup()
        {
            InitializeComponent();
            this.IsVisible = false;
        }

        public Task<bool> ShowAsync()
        {
            _tcs = new TaskCompletionSource<bool>();

            this.IsVisible = true;
            MessageLabel.Text = string.Empty;
            MessageLabel.TextColor = Colors.Red;

            CurrentPasswordEntry.Text = string.Empty;
            NewPasswordEntry.Text = string.Empty;
            ConfirmPasswordEntry.Text = string.Empty;

            CurrentPasswordEntry.Focus();

            return _tcs.Task;
        }

        private async void OnConfirmClicked(object sender, EventArgs e)
        {
            string pwd = CurrentPasswordEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(pwd))
            {
                MessageLabel.Text = "Vui lòng nhập mật khẩu.";
                MessageLabel.TextColor = Colors.Red;
                return;
            }

            bool result = await _adminPasswordDal.ValidateAdminPasswordAsync(pwd);

            if (result)
            {
                MessageLabel.Text = "Mật khẩu đúng.";
                MessageLabel.TextColor = Colors.Green;

                PasswordValidated?.Invoke(this, true);

                this.IsVisible = false;
                _tcs?.TrySetResult(true);
            }
            else
            {
                MessageLabel.Text = "Mật khẩu không đúng.";
                MessageLabel.TextColor = Colors.Red;

                PasswordValidated?.Invoke(this, false);
            }
        }

        private async void OnChangePasswordClicked(object sender, EventArgs e)
        {
            string currentPwd = CurrentPasswordEntry.Text?.Trim() ?? string.Empty;
            string newPwd = NewPasswordEntry.Text?.Trim() ?? string.Empty;
            string confirmPwd = ConfirmPasswordEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(currentPwd))
            {
                MessageLabel.Text = "Vui lòng nhập mật khẩu hiện tại.";
                MessageLabel.TextColor = Colors.Red;
                return;
            }

            if (string.IsNullOrWhiteSpace(newPwd))
            {
                MessageLabel.Text = "Mật khẩu mới không được rỗng.";
                MessageLabel.TextColor = Colors.Red;
                return;
            }

            if (!string.Equals(newPwd, confirmPwd, StringComparison.Ordinal))
            {
                MessageLabel.Text = "Mật khẩu mới và xác nhận không khớp.";
                MessageLabel.TextColor = Colors.Red;
                return;
            }

            bool validCurrent = await _adminPasswordDal.ValidateAdminPasswordAsync(currentPwd);
            if (!validCurrent)
            {
                MessageLabel.Text = "Mật khẩu hiện tại không đúng.";
                MessageLabel.TextColor = Colors.Red;
                return;
            }

            await _adminPasswordDal.SaveAdminPasswordAsync(newPwd);

            MessageLabel.Text = "Đổi mật khẩu thành công.";
            MessageLabel.TextColor = Colors.Green;

            NewPasswordEntry.Text = string.Empty;
            ConfirmPasswordEntry.Text = string.Empty;
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            this.IsVisible = false;
            Closed?.Invoke(this, EventArgs.Empty);
            _tcs?.TrySetResult(false);
        }
    }
}
