using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Region = Autodesk.AutoCAD.DatabaseServices.Region;

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

        [CommandMethod("KuroTL", CommandFlags.Session)]
        static public void KuroTL()
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

            doc.GetAcadDocument().GetType().InvokeMember("SendCommand",
                                        BindingFlags.InvokeMethod,
                                   null, doc.GetAcadDocument(), dataArry);
            if (IsCommandActive() == true)
            {
                dataArry[0] = "Yes ";
                doc.GetAcadDocument().GetType().InvokeMember(
                   "SendCommand",
                   BindingFlags.InvokeMethod,
                   null, doc.GetAcadDocument(), dataArry

                 );
            }
            db.ObjectAppended -=
                            new ObjectEventHandler(Database_ObjectAppended);
            using (DocumentLock lock1 = doc.LockDocument())
            {
                using (Transaction ta =
                                    db.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId id in collection)
                    {
                        if (id.IsErased == true)
                        {
                            continue;
                        }
                        DBObject obj = ta.GetObject(id,
                                                       OpenMode.ForRead);
                        if (obj is Polyline)
                        {
                            Polyline pl = (Polyline)obj;
                            //add mtext at the center region pl
                            MText mt = new MText();
                            mt.Contents = pl.Area.ToString("F2");
                            mt.Location = result.Value;
                            mt.Rotation = 3*Math.PI/2; //to radians
                            mt.Height = 2;

                            //add the mtext to the model space
                            BlockTableRecord btr =
                                (BlockTableRecord)ta.GetObject(
                                    db.CurrentSpaceId,
                                    OpenMode.ForWrite);
                            btr.AppendEntity(mt);
                            ta.AddNewlyCreatedDBObject(mt, true);

                        }
                        else if (obj is
                                    Region)
                        {
                            Region rg =
                               (Region)obj;
                            ed.WriteMessage("area is "
                                            + rg.Area.ToString() + "\n");
                        }
                    }
                    ta.Commit();
                }
            }

        }

        static void Database_ObjectAppended(object sender, ObjectEventArgs e)
        {
            collection.Add(e.DBObject.ObjectId);
        }
    }
}
