using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace FacturacionDobleEje
{
    public class FirstLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;

            if (string.IsNullOrWhiteSpace(text))
                return "";

            // Divide por saltos de línea
            var firstLine = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)[0];

            return firstLine.Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
