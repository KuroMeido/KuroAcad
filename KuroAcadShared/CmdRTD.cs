
using KuroAcad.UI;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(KuroAcad.CmdRTD))]
namespace KuroAcad
{
    class CmdRTD
    {
        [CommandMethod("KuroRTD")]
        public void KuroRTD()
        {
            // Get active Document and Database
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acDb = acDoc.Database;

            //Show dialog
            KuroTLWPF kuroTLWPF = new KuroTLWPF();
            kuroTLWPF.ShowDialog();

            //Get list of polyline from user
            List<Polyline> pls = new List<Polyline>();
            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.AllowDuplicates = false;
            pso.AllowSubSelections = false;
            pso.MessageForAdding = "\nChọn lô đất";

            PromptSelectionResult psr = acDoc.Editor.GetSelection(pso);
            if (psr.Status != PromptStatus.OK || psr.Value.Count == 0)
            {
                return;
            }
            //Get a block attribute from user
            PromptEntityOptions peo = new PromptEntityOptions("\nChọn block attribute: ");
            peo.SetRejectMessage("\nKhông phải block attribute");
            peo.AddAllowedClass(typeof(BlockReference), true);
            PromptEntityResult per = acDoc.Editor.GetEntity(peo);
            if (per.Status != PromptStatus.OK)
                return;

            //Get prefix from user
            PromptStringOptions psoStr = new PromptStringOptions("\nNhập prefix: ");
            PromptResult prStr = acDoc.Editor.GetString(psoStr);
            if (prStr.Status != PromptStatus.OK)
                return;
            string prefix = prStr.StringResult;

            //Get start number from user
            PromptIntegerOptions pio = new PromptIntegerOptions("\nNhập số bắt đầu: ");
            pio.AllowNegative = false;
            pio.AllowZero = true;
            pio.DefaultValue = 0;
            PromptIntegerResult pir = acDoc.Editor.GetInteger(pio);
            if (pir.Status != PromptStatus.OK)
                return;
            int startNumber = pir.Value;

            using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
            {
                BlockReference br = (BlockReference)acDb.TransactionManager.GetObject(per.ObjectId, OpenMode.ForRead);

                foreach (SelectedObject so in psr.Value)
                {
                    Entity ent = (Entity)acTrans.GetObject(so.ObjectId, OpenMode.ForRead);
                    if (ent is Polyline)
                    {
                        Polyline pl = ent as Polyline;
                        pls.Add(pl);
                    }
                }

                //Insert block attribute to polyline in list pls
                foreach (Polyline pl in pls)
                {
                    //Get center point of polyline
                    Point3d cenPt = KuroExtensions.GetCenterPoint(pl);

                    //Get area of polyline
                    double area = pl.Area;

                    //Get name of polyline
                    string name = prefix + startNumber.ToString();

                    //Copy block attribute selected by user to center point of polyline
                    KuroExtensions.CopyEntities(acDb, acTrans, br, cenPt);

                    startNumber++;
                }

                //Commit transaction
                acTrans.Commit();
            }

        }
    }
}
