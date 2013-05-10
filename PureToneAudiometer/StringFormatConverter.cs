
namespace PureToneAudiometer
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var format = parameter as string;
            if (value == null || format == null)
                return value;

            return string.Format(culture, format, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
