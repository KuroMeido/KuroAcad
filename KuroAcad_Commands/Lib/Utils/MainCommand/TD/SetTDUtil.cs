using KuroAcad.Helper;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace KuroAcad
{
    internal static class SetTDUtil
    {
        internal static void SetTD()
        {
            // Get active Document and Database
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acDb = acDoc.Database;

            // Start transaction
            using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
            {
                // Get selection
                PromptSelectionOptions pso = new PromptSelectionOptions();
                pso.MessageForAdding = "\nChọn Bảng thống kê tọa độ: ";
                pso.SingleOnly = true;

                PromptSelectionResult psr = acDoc.Editor.GetSelection(pso);
                if (psr.Status != PromptStatus.OK || psr.Value.Count == 0)
                {
                    acDoc.Editor.WriteMessage("\nVui lòng chọn đối tượng trước khi thực hiện.");
                    return;
                }

                // Lookup column "X" "Y" in table
                Table acTable = acTrans.GetObject(psr.Value[0].ObjectId, OpenMode.ForRead) as Table;
                int XcolumnIndex = BuildTableHelper.LookupValueInTable(acTable, "X");
                int YcolumnIndex = BuildTableHelper.LookupValueInTable(acTable, "Y");

                // Get list value from XY column
                List<Point2d> pts = new List<Point2d>();

                int rowNum = acTable.Rows.Count();
                int rowIdx = 5;
                for (int i = 0; i < rowNum - 5; i++)
                {
                    if (acTable.Cells[rowIdx, XcolumnIndex].Value == null ||
                        acTable.Cells[rowIdx, YcolumnIndex].Value == null)
                    {
                        continue;
                    }

                    pts.Add(
                        new Point2d(
                            Convert.ToDouble(acTable.Cells[rowIdx, XcolumnIndex].Value),
                            Convert.ToDouble(acTable.Cells[rowIdx, YcolumnIndex].Value)));

                    rowIdx++;
                }

                // Create Polyline from list pts
                Polyline pl = new Polyline();
                pl.SetDatabaseDefaults();
                pl.ColorIndex = 1;
                pl.Closed = true;
                pl.AddVertexAt(0, pts[0], 0, 0, 0);

                for (int i = 1; i < pts.Count; i++)
                {
                    pl.AddVertexAt(i, pts[i], 0, 0, 0);
                }

                // Add polyline to database
                BlockTable acBlkTbl = acTrans.GetObject(acDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec = acTrans.GetObject(
                    acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                acBlkTblRec.AppendEntity(pl);
                acTrans.AddNewlyCreatedDBObject(pl, true);

                acTrans.Commit();
            }
        }
    }
}