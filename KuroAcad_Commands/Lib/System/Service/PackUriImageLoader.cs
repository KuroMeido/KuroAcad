using System.Collections.Concurrent;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KuroAcad
{
    internal sealed class PackUriImageLoader : IImageLoader
    {
        private readonly string assemblyName;
        private readonly ConcurrentDictionary<string, ImageSource?> cache = new();

        public PackUriImageLoader(Type ownerType)
        {
            assemblyName = ownerType.Assembly.GetName().Name ?? string.Empty;
        }

        public ImageSource? Load(string iconPath)
        {
            if (string.IsNullOrWhiteSpace(iconPath))
            {
                return null;
            }

            return cache.GetOrAdd(iconPath, CreateImage);
        }

        private ImageSource? CreateImage(string iconPath)
        {
            try
            {
                var uri = new Uri(
                    $"pack://application:,,,/{assemblyName};component/{iconPath}",
                    UriKind.Absolute);

                var bitmap = new BitmapImage(uri);
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}