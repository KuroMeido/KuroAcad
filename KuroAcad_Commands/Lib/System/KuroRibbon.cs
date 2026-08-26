using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace KuroAcad
{
    internal static class KuroRibbon
    {
        private static readonly IKuroRibbonService RibbonService =
            new KuroRibbonService(
                new PackUriImageLoader(typeof(KuroRibbon)),
                new AutoCadCommandExecutor());

        public static void CreateMyRibbon()
        {
            RibbonService.Create();
        }
    }
}