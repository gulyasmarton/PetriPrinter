using GCodeAPI;
using PetriPrinter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace PetriPrinter.View
{
    [ValueConversion(typeof(List<PetriTask>), typeof(string))]
    public class PetriTaskListStringConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter,
                      System.Globalization.CultureInfo culture)
        {
            var list = value as List<PetriTask>;
            if (list == null)
                return "-";
            return string.Join(", ", list);

        }
        public object ConvertBack(object value, Type targetType, object parameter,
                        System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
