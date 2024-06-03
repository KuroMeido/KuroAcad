[assembly: ExtensionApplication(typeof(KuroAcad2025.ExtensionApplication))]

namespace KuroAcad2025
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
                doc.Editor.WriteMessage("\nKuroAcad2025 loaded.\n");
            }
        }

        public void Terminate()
        { }
    }
}
