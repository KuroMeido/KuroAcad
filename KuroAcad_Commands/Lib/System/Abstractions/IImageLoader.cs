using System.Windows.Media;

namespace KuroAcad
{
    internal interface IImageLoader
    {
        ImageSource? Load(string iconPath);
    }
}