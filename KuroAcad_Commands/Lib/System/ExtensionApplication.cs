using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: ExtensionApplication(typeof(KuroAcad.ExtensionApplication))]

namespace KuroAcad
{
    internal class ExtensionApplication : IExtensionApplication
    {
        public void Initialize()
        {
            System.Windows.Application.ResourceAssembly ??= typeof(ExtensionApplication).Assembly;
            KuroRibbon.CreateMyRibbon();
            Application.Idle += OnIdle;
        }

        private static void OnIdle(object? sender, EventArgs e)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                Application.Idle -= OnIdle;
                doc.Editor.WriteMessage("\nKuroAcad loaded.\n");
            }
        }

        public void Terminate()
        { }
    }
}
