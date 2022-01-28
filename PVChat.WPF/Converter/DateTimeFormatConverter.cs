using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PVChat.WPF.Converter
{
    [ValueConversion(typeof(string), typeof(Date))]
    class DateTimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = (DateTime)value;
            if (date.Year != DateTime.Today.Year)
            {
                return Date.NotThisYear;
            }
            else if(date.Date != DateTime.Today)
            {
                return Date.NotToday;
            }
            else 
            {
                return Date.Today;
            }
            
           
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public enum Date
    {
        Today,
        NotToday,
        NotThisYear
    }
}
