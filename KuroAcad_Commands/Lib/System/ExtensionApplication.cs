using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: ExtensionApplication(typeof(KuroAcad.ExtensionApplication))]

namespace KuroAcad
{
    internal class ExtensionApplication : IExtensionApplication
    {
        private const int MaxRibbonRetryCount = 100;
        private int ribbonRetryCount;
        private bool ribbonRegistered;
        private bool startupMessageShown;

        public void Initialize()
        {
            System.Windows.Application.ResourceAssembly ??= typeof(ExtensionApplication).Assembly;
            Application.Idle += OnIdle;
        }

        private void OnIdle(object? sender, EventArgs e)
        {
            if (!ribbonRegistered && ribbonRetryCount < MaxRibbonRetryCount)
            {
                ribbonRegistered = KuroRibbon.TryCreate();
                ribbonRetryCount++;
            }

            var doc = Application.DocumentManager.MdiActiveDocument;
            if (!startupMessageShown && doc != null)
            {
                doc.Editor.WriteMessage("\nKuroAcad loaded.\n");
                startupMessageShown = true;
            }

            if (startupMessageShown && (ribbonRegistered || ribbonRetryCount >= MaxRibbonRetryCount))
            {
                Application.Idle -= OnIdle;
            }
        }

        public void Terminate()
        { }
    }
}
