using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace KuroAcad.UI
{
    /// <summary>
    /// Provides the conversion method to get the layer color
    /// </summary>
    [ValueConversion(typeof(ICustomTypeDescriptor), typeof(SolidColorBrush))]

    class LayerColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts an Autodesk.AutoCAD.Colors.Color object representing the color of a layer to an instance of System.Media.SolidColorBrush
        /// </summary>
        /// <param name="value">The color to convert.</param>
        /// <param name="targetType">SolidColorBrush type</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>A SolidColorBrush instance representing the color of the layer.</returns>
        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is Autodesk.AutoCAD.Colors.Color)
            {
                var acadColor = (Autodesk.AutoCAD.Colors.Color)value;
                var drawingColor = acadColor.ColorValue;
                var mediaColor = Color.FromRgb(drawingColor.R, drawingColor.G, drawingColor.B);
                return new SolidColorBrush(mediaColor);
            }
            return null;
        }

        /// <summary>
        /// Reverse conversion method not used.
        /// </summary>
        /// <returns>Always null</returns>
        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
