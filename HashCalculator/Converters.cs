using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace HashCalculator;

internal class CmpResFgCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object param, CultureInfo culture)
    {
        if (!Settings.Current.ShowResultText)
            return "Transparent";
        return (CmpRes)value switch
        {
            CmpRes.Unrelated => "Black",
            CmpRes.Matched => "White",
            CmpRes.Mismatch => "White",
            CmpRes.Uncertain => "White",
            _ => "Transparent",
        };
    }

    public object ConvertBack(object value, Type targetType, object param, CultureInfo culture)
    {
        return CmpRes.Unrelated; // 此处未使用，只返回默认值
    }
}

internal class CmpResBgCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object param, CultureInfo culture)
    {
        return (CmpRes)value switch
        {
            CmpRes.Unrelated => "#64888888",
            CmpRes.Matched => "ForestGreen",
            CmpRes.Mismatch => "Red",
            CmpRes.Uncertain => "Black",
            _ => "Transparent",
        };
    }

    public object ConvertBack(object value, Type targetType, object param, CultureInfo culture)
    {
        return CmpRes.Unrelated; // 此处未使用，只返回默认值
    }
}

internal class CmpResTextCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object param, CultureInfo culture)
    {
        return (CmpRes)value switch
        {
            CmpRes.Unrelated => "无关联",
            CmpRes.Matched => "已匹配",
            CmpRes.Mismatch => "不匹配",
            CmpRes.Uncertain => "不确定",
            _ => string.Empty,
        };
    }

    public object ConvertBack(object value, Type targetType, object param, CultureInfo culture)
    {
        return CmpRes.NoResult; // 此处未使用，只返回默认值
    }
}

internal class CmpResBorderCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object param, CultureInfo culture)
    {
        if (Settings.Current.ShowResultText)
            return "0";
        else
            return "3";
    }

    public object ConvertBack(object value, Type targetType, object param, CultureInfo culture)
    {
        return CmpRes.NoResult; // 此处未使用，只返回默认值
    }
}

internal class AlgoTypeBgCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (AlgoType)value switch
        {
            AlgoType.SHA1 => "#64FF0071",
            AlgoType.SHA224 => "#64331772",
            AlgoType.SHA256 => "#640066FF",
            AlgoType.SHA384 => "#64FFBB33",
            AlgoType.SHA512 => "#64008B73",
            AlgoType.MD5 => "#64799B00",
            _ => "#64FF0000",
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return AlgoType.SHA256; // 此处未使用，只返回默认值
    }
}

internal class AlgoTypeNameCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((AlgoType)value == AlgoType.Unknown)
            return "待定";
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class VisibRunningCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        HashState state = (HashState)value;
        if (state != HashState.Running && state != HashState.Paused)
            return Visibility.Hidden;
        else
            return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class PauseBtnTextCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        HashState state = (HashState)value;
        if (state == HashState.Running)
            return "暂停...";
        else
            return "继续...";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class PauseBtnImgsrcCvt : IValueConverter
{
    private readonly BitmapImage paused =
        new(new Uri("/Images/pause.png", UriKind.Relative));
    private readonly BitmapImage noPaused =
        new(new Uri("/Images/continue.png", UriKind.Relative));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        HashState state = (HashState)value;
        if (state == HashState.Running)
            return paused;
        else
            return noPaused;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class VisibNotCalcCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        HashState state = (HashState)value;
        if (state == HashState.Running || state == HashState.Paused)
            return Visibility.Hidden;
        else
            return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class VisibWaitingCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((HashState)value != HashState.Waiting)
            return Visibility.Hidden;
        else
            return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class VisibSucceededCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((HashResult)value != HashResult.Succeeded)
            return Visibility.Hidden;
        else
            return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class VisibCanceledCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((HashResult)value != HashResult.Canceled)
            return Visibility.Hidden;
        else
            return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class VisibTotalProgressCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((QueueState)value != QueueState.Started)
            return Visibility.Hidden;
        else
            return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class ButtonEnableCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((QueueState)value != QueueState.Started)
            return true;
        else
            return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class ButtonNotEnableCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((QueueState)value == QueueState.Started)
            return true;
        else
            return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class NoColumnCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
            return Visibility.Hidden;
        else
            return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class ProgressWidthCvt : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (AlgoType)value switch
        {
            AlgoType.SHA1 => 270D,
            AlgoType.SHA224 => 380D,
            AlgoType.SHA256 => 430D,
            AlgoType.SHA384 => 650D,
            AlgoType.SHA512 => 860D,
            _ => (object)210D,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
