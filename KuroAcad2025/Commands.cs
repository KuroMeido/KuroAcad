[assembly: CommandClass(typeof(KuroAcad2025.Commands))]

namespace KuroAcad2025
{
    public class Commands
    {
        [CommandMethod("Test")]
        public void Test()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            ed.WriteMessage("\nwsez");
            if (IsSavedFile() == true)
            {
                ed.WriteMessage("\nfile have been saved");
            }
            else
            {
                ed.WriteMessage("\nFile is new");
            }
            using var tr = db.TransactionManager.StartTransaction();

            tr.Commit();
        }
        private Boolean IsSavedFile()
        {
            int result = System.Convert.ToInt16(Application.GetSystemVariable("DWGTITLED"));
            if (result != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
