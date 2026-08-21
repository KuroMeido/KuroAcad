using Autodesk.AutoCAD.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace KuroAcad.UI
{
    internal class CustomPaletteSet : PaletteSet
    {
        // champ statique
        static bool wasVisible;

        /// <summary>
        /// Creates a new instance of CustomPaletteSet 
        /// </summary>
        public CustomPaletteSet()
            : base("Palette WPF", "KuroPalette", new Guid("{D68220B1-0665-405E-A2C4-D450FF103644}"))
        {
            Style =
                PaletteSetStyles.ShowAutoHideButton |
                PaletteSetStyles.ShowCloseButton |
                PaletteSetStyles.ShowPropertiesMenu;
            MinimumSize = new System.Drawing.Size(250, 150);
            AddVisual("Cercle", new PaletteTabView());

            // automatic hiding of the palette when no instance of Document is active (no document state)
            var docs = Application.DocumentManager;
            docs.DocumentBecameCurrent += (s, e) =>
                Visible = e.Document == null ? false : wasVisible;
            docs.DocumentCreated += (s, e) =>
                Visible = wasVisible;
            docs.DocumentToBeDeactivated += (s, e) =>
                wasVisible = Visible;
            docs.DocumentToBeDestroyed += (s, e) =>
            {
                wasVisible = Visible;
                if (docs.Count == 1)
                    Visible = false;
            };
        }
    }
}
