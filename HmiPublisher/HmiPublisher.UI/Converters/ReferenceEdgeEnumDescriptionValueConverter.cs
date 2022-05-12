using HmiPublisher.Model;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace HmiPublisher.UI.Converters
{
    public class ReferenceEdgeEnumDescriptionValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {


            if (!(value is Compression))
            {
                return " - ";
            }

            var type = typeof(Compression);
            var name = Enum.GetName(type, value);
            FieldInfo fi = type.GetField(name);
            var descriptionAttrib = (DescriptionAttribute)
                Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

            return descriptionAttrib.Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
