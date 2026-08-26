using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace KuroAcad
{
    internal sealed class AutoCadCommandExecutor : IAcadCommandExecutor
    {
        public void Execute(string command)
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            if (document == null)
            {
                return;
            }

            document.SendStringToExecute($"\x03\x03{command} ", true, false, true);
        }

        public void WriteInfo(string message)
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            if (document == null)
            {
                return;
            }

            document.Editor.WriteMessage($"\n[KuroAcad] {message}");
        }
    }
}