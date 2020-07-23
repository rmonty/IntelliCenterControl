using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;

namespace IntelliCenterControl.Views
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan)
            {
                return DateTime.Today.Add((TimeSpan)value);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
            {
                return ((DateTime)value).TimeOfDay;
            }
            return 0;
        }
    }

    public class IntEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum)
            {
                return (int)value;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                return Enum.ToObject(targetType, value);
            }
            return 0;
        }
    }

    public class EnumToCheckedConverter : IValueConverter
{
    public Type Type { get; set; }
    public int? LastValue { get; private set; }
    public bool Flags { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null && value.GetType() == Type)
        {
            try
            {
                var parameterValue = Enum.Parse(Type, parameter as string);

                if (Flags == true)
                {
                    var intParameter = (int)parameterValue;
                    var intValue = (int)value;
                    LastValue = intValue;

                    return (intValue & intParameter) == intParameter;
                }
                else
                {
                    return Equals(parameterValue, value);
                }
            }
            catch (ArgumentNullException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                throw new NotSupportedException();
            }
        }
        else if (value == null)
        {
            return false;
        }

        throw new NotSupportedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null && value is bool check)
        {
            if (check)
            {
                try
                {
                    if (Flags == true && LastValue.HasValue)
                    {
                        var parameterValue = Enum.Parse(Type, parameter as string);
                        var intParameter = (int)parameterValue;

                        return Enum.ToObject(Type, LastValue | intParameter);
                    }
                    else
                    {
                        return Enum.Parse(Type, parameter as string);
                    }
                }
                catch (ArgumentNullException)
                {
                    return Binding.DoNothing;
                }
                catch (ArgumentException)
                {
                    return Binding.DoNothing;
                }
            }
            else
            {
                try
                {
                    if (Flags == true && LastValue.HasValue)
                    {
                        var parameterValue = Enum.Parse(Type, parameter as string);
                        var intParameter = (int)parameterValue;

                        return Enum.ToObject(Type, LastValue ^ intParameter);
                    }
                    else
                    {
                        return Binding.DoNothing;
                    }
                }
                catch (ArgumentNullException)
                {
                    return Binding.DoNothing;
                }
                catch (ArgumentException)
                {
                    return Binding.DoNothing;
                }
            }
        }

        throw new NotSupportedException();
    }
}

    public class VisibleEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }

            if (value is Enum && parameter is string stringParameter)
            {
                if (stringParameter.Contains("|"))
                {
                    var parameterArray = new List<string>(stringParameter.Split('|'));

                    if(parameterArray.Contains(((int) value).ToString()))
                    {
                        return true;
                    }
                }
                else if (int.TryParse(stringParameter, out var intParameter))
                {
                    return (int) value == intParameter;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if (value is bool)
            //{
            //    return Enum.ToObject(targetType, value);
            //}
            return 0;
        }
    }

    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
                
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
