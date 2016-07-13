using System;
using System.Globalization;
using System.Windows.Data;

namespace ESD_Download_Links_Viewer
{
    [ValueConversion(typeof(ulong), typeof(string))]
    public class SizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ulong bytesValue = (ulong)value;
            double KBytesValue = (double)bytesValue / 1024;

            return KBytesValue.ToString("N", culture) + " KB";
        }

        //Actually, this method's implementation is not necessary, because I just using OneWay binding mode.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string valueStr = value.ToString();
            valueStr = valueStr.Replace("KB", "").Trim();

            double doubleValue;
            if (double.TryParse(valueStr, NumberStyles.Any, culture, out doubleValue))
            {
                return (ulong)(doubleValue * 1024);
            }
            else
            {
                return 0;
            }
        }
    }
}
