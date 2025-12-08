using System;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls;

namespace UziSport.Controls
{
    public class NumericEntry : Entry
    {
        private bool _isInternalUpdate;

        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create(
                nameof(Value),
                typeof(int?),
                typeof(NumericEntry),
                null,
                BindingMode.TwoWay,
                propertyChanged: OnValueChanged);

        /// <summary>
        /// Giá trị số (0..999,999,999). Text rỗng => Value = 0.
        /// </summary>
        public int? Value
        {
            get => (int?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public NumericEntry()
        {
            Keyboard = Keyboard.Numeric;
            TextChanged += OnTextChangedInternal;
            HorizontalTextAlignment = TextAlignment.End;
        }

        private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (NumericEntry)bindable;

            if (control._isInternalUpdate)
                return;

            control._isInternalUpdate = true;

            if (newValue is int intValue)
            {
                if (intValue < 0) intValue = 0;
                if (intValue > 999_999_999) intValue = 999_999_999;

                control.Text = intValue.ToString("###,###,##0", CultureInfo.InvariantCulture);
            }
            else
            {
                // Nếu Value = null từ ViewModel thì cho text rỗng
                control.Text = string.Empty;
            }

            control._isInternalUpdate = false;
        }

        private void OnTextChangedInternal(object sender, TextChangedEventArgs e)
        {
            if (_isInternalUpdate)
                return;

            var text = e.NewTextValue;

            // Khi text rỗng => Value = 0
            if (string.IsNullOrWhiteSpace(text))
            {
                _isInternalUpdate = true;
                Text = string.Empty;
                Value = 0;   // <-- đổi từ null thành 0
                _isInternalUpdate = false;
                return;
            }

            var digitsOnly = new string(text.Where(char.IsDigit).ToArray());

            // Không còn số nào hợp lệ => Value = 0
            if (string.IsNullOrEmpty(digitsOnly))
            {
                _isInternalUpdate = true;
                Text = string.Empty;
                Value = 0;   // <-- đổi từ null thành 0
                _isInternalUpdate = false;
                return;
            }

            // Parse sang int, giới hạn 0..999,999,999
            if (!int.TryParse(digitsOnly, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
            {
                number = 0;
            }

            if (number < 0) number = 0;
            if (number > 999_999_999) number = 999_999_999;

            var formatted = number.ToString("###,###,##0", CultureInfo.InvariantCulture);

            _isInternalUpdate = true;
            Text = formatted;
            CursorPosition = Text?.Length ?? 0;
            Value = number;
            _isInternalUpdate = false;
        }
    }
}
