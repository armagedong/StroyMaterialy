using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using StroyMaterialy.Models;

namespace StroyMaterialy.Converters;

public class DiscountRowConverter : IValueConverter
{
    public static readonly Brush HighDiscountBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F4A460")!);
    public static readonly Brush NormalBrush = Brushes.White;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Product p && p.HasHighDiscount)
            return HighDiscountBrush;
        return NormalBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
