using System;
using System.Globalization;

namespace SistemaParamedicosDemo4.Converters
{
    /// <summary>
    /// Convierte un valor booleano inverso para mostrar/ocultar elementos
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }

    /// <summary>
    /// Convierte una URL de imagen a ImageSource
    /// Retorna null si la URL es inválida (permitiendo usar FallbackValue)
    /// </summary>
    public class UrlToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    // Validar que sea una URL válida
                    if (url.StartsWith("http://") || url.StartsWith("https://"))
                    {
                        return ImageSource.FromUri(new Uri(url));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al cargar imagen: {ex.Message}");
                }
            }
            return null; // Permite que el FallbackValue se active
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Determina si una URL de foto es válida
    /// </summary>
    public class HasValidPhotoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                return url.StartsWith("http://") || url.StartsWith("https://");
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}