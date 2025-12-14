using System.Globalization;

namespace UziSport.Converters;

public class NegativeNumberToColorConverter : IValueConverter
{
    public Color NegativeColor { get; set; } = Colors.DarkRed;
    public Color NonNegativeColor { get; set; } = Colors.Black;
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return NonNegativeColor;

        try
        {
            // ProfitAmount của bạn có thể là int/long/decimal/double...
            var dec = System.Convert.ToDecimal(value, culture);
            return dec < 0 ? NegativeColor : NonNegativeColor;
        }
        catch
        {
            return NonNegativeColor;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
