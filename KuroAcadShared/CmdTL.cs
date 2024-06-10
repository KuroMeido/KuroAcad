using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(KuroAcad.CmdTL))]
namespace KuroAcad
{
    class CmdTL
    {
        static ObjectIdCollection collection = new ObjectIdCollection();

        static string commandName = "";

        static bool IsCommandActive()
        {
            String str = (String)Application.GetSystemVariable("CMDNAMES");
            if (String.Compare(commandName, str, true) != 0)
            {
                return true;
            }
            return false;
        }

        [CommandMethod("BoundaryCommandLine", CommandFlags.Session)]

        static public void BoundaryCommandLine()

        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptPointOptions options =
                            new PromptPointOptions("Pick a point");
            PromptPointResult result = ed.GetPoint(options);

            if (result.Status != PromptStatus.OK)
                return;

            double x = result.Value.X;
            double y = result.Value.Y;
            string strPt = x.ToString() + "," + y.ToString();

            //put the database
            Database db = doc.Database;
            collection.Clear();

            commandName = (String)Application.GetSystemVariable("CMDNAMES");
            db.ObjectAppended +=
                            new ObjectEventHandler(Database_ObjectAppended);

            //run the boundary command using ActiveX send command.

            object[] dataArry = new object[1];
            dataArry[0] = "-boundary " + strPt + "  ";

            doc.SendStringToExecute("-boundary " + strPt + " ", true, false, false);
            //doc.GetType().InvokeMember("SendCommand",
            //                                    BindingFlags.InvokeMethod,
            //                               null, doc, dataArry);

            if (IsCommandActive() == true)
            {
                dataArry[0] = "Yes ";
                doc.GetType().InvokeMember(
                   "SendCommand",
                   BindingFlags.InvokeMethod,
                   null, doc, dataArry
                 );
            }

            db.ObjectAppended -=
                            new ObjectEventHandler(Database_ObjectAppended);

        }

        static void Database_ObjectAppended(object sender, ObjectEventArgs e)
        {
            collection.Add(e.DBObject.ObjectId);
        }
    }
}
