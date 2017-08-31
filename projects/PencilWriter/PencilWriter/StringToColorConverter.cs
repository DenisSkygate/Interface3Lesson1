using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace PencilWriter
{
    class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, String culture)
        {
            String text = (String)value;
            if (text=="")
                return new SolidColorBrush(Color.FromArgb(255, 51, 204, 51));
            else
                return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        }
        public object ConvertBack(object value, Type targetType, object parameter, String culture)
        {
            throw new NotImplementedException();
        }
    }
}
