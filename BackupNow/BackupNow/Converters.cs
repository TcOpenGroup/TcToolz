using BackupNow.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BackupNow
{

    public class BoolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if ((bool)value)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;

            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class AppThemeEnumDescriptionValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = typeof(AppTheme);
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


    public class IncludeToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if ((bool)value)
            {
                return 1.0;
            }
            else
            {
                return 0.5;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class ProgressMessageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if ((bool)value)
            {
                return Brushes.Red;
            }
            else
            {
                if (ModernWpf.ThemeManager.Current.ActualApplicationTheme == ModernWpf.ApplicationTheme.Dark)
                {
                    return Brushes.White;
                }
                else
                {
                    return Brushes.Black;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class ProgressBarColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if ((bool)value)
            {
                return Brushes.Orange;
            }
            else
            {

                return (SolidColorBrush)new BrushConverter().ConvertFrom("#00d700");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
