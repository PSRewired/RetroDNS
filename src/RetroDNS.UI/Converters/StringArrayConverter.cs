using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace RetroDNS.UI.Converters;

public class StringArrayConverter : IValueConverter
{
    public static readonly StringArrayConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<string> stringList)
        {
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        return string.Join('\n', stringList);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return (value as string)!.Split('\n');
    }
}
