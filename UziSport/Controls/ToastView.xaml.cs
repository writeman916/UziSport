using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace UziSport.Controls
{
    public partial class ToastView : ContentView
    {
        private CancellationTokenSource _cts;

        public enum ToastKind
        {
            Info,
            Success,
            Warning,
            Error
        }

        private static readonly Color TOASTCOLOR_INFO = Color.FromArgb("#42A5F5");
        private static readonly Color TOASTCOLOR_SUCCESS = Color.FromArgb("#66BB6A");
        private static readonly Color TOASTCOLOR_WARNING = Color.FromArgb("#FFCA28");
        private static readonly Color TOASTCOLOR_ERROR = Color.FromArgb("#EF5350");

        public ToastView()
        {
            InitializeComponent();
            IsVisible = false;
            Opacity = 0;
        }

        public async Task ShowAsync(ToastKind toastKind, string message, int durationMs = 2000)
        {
            // Hủy toast trước nếu đang chạy
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            MessageLabel.Text = message;
            IsVisible = true;
            Opacity = 0;

            // Chọn màu + icon theo loại
            string iconText = "";
            Color bgColor = TOASTCOLOR_INFO;

            switch (toastKind)
            {
                case ToastKind.Info:
                    bgColor = TOASTCOLOR_INFO;
                    iconText = "ℹ";      // thông tin
                    break;

                case ToastKind.Success:
                    bgColor = TOASTCOLOR_SUCCESS;
                    iconText = "✔";      // thành công
                    break;

                case ToastKind.Warning:
                    bgColor = TOASTCOLOR_WARNING;
                    iconText = "⚠";      // cảnh báo
                    break;

                case ToastKind.Error:
                    bgColor = TOASTCOLOR_ERROR;
                    iconText = "✖";      // lỗi
                    break;
            }

            ToastFrame.BackgroundColor = bgColor;
            IconLabel.Text = iconText;
            IconLabel.TextColor = Colors.White;
            MessageLabel.TextColor = Colors.White;

            // Fade in
            await this.FadeTo(1, 200);

            try
            {
                await Task.Delay(durationMs, token);
            }
            catch (TaskCanceledException)
            {
                // Bị toast mới đè, không cần fade out
                return;
            }

            if (!token.IsCancellationRequested)
            {
                // Fade out
                await this.FadeTo(0, 200);
                IsVisible = false;
            }
        }
    }
}
