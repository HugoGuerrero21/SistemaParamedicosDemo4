using System.Globalization;

namespace SistemaParamedicosDemo4.Converters
{
    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var url = value as string;

                // Si no hay URL, devolver imagen placeholder
                if (string.IsNullOrEmpty(url))
                {
                    // Puedes poner aquí el nombre de tu imagen placeholder en Resources
                    return ImageSource.FromFile("placeholder_product.png");
                }

                // Si es una URL válida, devolverla
                if (url.StartsWith("http://") || url.StartsWith("https://"))
                {
                    return ImageSource.FromUri(new Uri(url));
                }

                // Si es una ruta de archivo local
                return ImageSource.FromFile(url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar imagen: {ex.Message}");
                return ImageSource.FromFile("placeholder_product.png");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}