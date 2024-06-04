[assembly: ExtensionApplication(typeof(KuroAcad.ExtensionApplication))]

namespace KuroAcad
{
    internal class ExtensionApplication : IExtensionApplication
    {
        public void Initialize()
        {

            Application.Idle += OnIdle;

        }

        private void OnIdle(object? sender, EventArgs e)
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
