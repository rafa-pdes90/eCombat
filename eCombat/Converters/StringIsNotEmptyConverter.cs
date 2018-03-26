using System;
using System.Globalization;
using System.Windows.Data;

namespace eCombat.Converters
{
    public class StringIsNotEmptyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = true;

            foreach (object value in values)
            {
                switch (value)
                {
                    case string v:
                        result = result && v.Length > 0;
                        break;
                    case bool v:
                        result = result && v;
                        break;
                }
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
