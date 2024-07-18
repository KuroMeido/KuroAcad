using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(KuroAcad.CmdGetTD))]
namespace KuroAcad
{
    class CmdGetTD
    {
        [CommandMethod("KGetTD")]
        public void KuroGetTD()
        {
            // Get active Document and Database
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acDb = acDoc.Database;

            // Start transaction
            using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
            {
                // Get number from user
                PromptIntegerOptions pio = new PromptIntegerOptions("\nNhập số chữ số sau dấu phẩy: ");
                pio.AllowNegative = false;
                pio.AllowZero = false;
                pio.DefaultValue = 2;
                PromptIntegerResult pir = acDoc.Editor.GetInteger(pio);

                if (pir.Status != PromptStatus.OK)
                    return;

                // Get selection
                PromptSelectionOptions pso = new PromptSelectionOptions();
                PromptSelectionResult psr = acDoc.Editor.GetSelection(pso);
                if (psr.Status != PromptStatus.OK || psr.Value.Count == 0)
                {
                    acDoc.Editor.WriteMessage("\nChọn đường polyline");
                    return;
                }

                //Get pointfrom user
                PromptPointResult ppr = acDoc.Editor.GetPoint("\nChọn điểm đặt bảng tọa độ: ");
                if (ppr.Status != PromptStatus.OK)
                    return;

                // Get list of points from polyline
                List<Point3d> pts = new List<Point3d>();
                foreach (SelectedObject so in psr.Value)
                {
                    Entity ent = (Entity)acTrans.GetObject(so.ObjectId, OpenMode.ForRead);
                    if (ent is Polyline)
                    {
                        Polyline pl = ent as Polyline;
                        for (int i = 0; i < pl.NumberOfVertices; i++)
                        {
                            pts.Add(pl.GetPoint3dAt(i));
                        }
                    }
                }

                //Get list line from polyline
                List<Line> lines = new List<Line>();
                for (int i = 0; i < pts.Count - 1; i++)
                {
                    Line line = new Line(pts[i], pts[i + 1]);
                    lines.Add(line);
                }

                //Create Table
                Table acTable = new Table();
                acTable.TableStyle = acDoc.Database.Tablestyle;
                acTable.Position = ppr.Value;
                acTable.NumRows = 5;
                acTable.NumColumns = 4;

                //Set Title
                acTable.Cells[0, 0].TextString = "BẢNG THỐNG KÊ TỌA ĐỘ GỐC RANH";
                acTable.Cells[0, 0].Alignment = CellAlignment.MiddleCenter;

                //Set header
                acTable.Cells[2, 0].TextString = "Số hiệu điểm";
                acTable.Cells[2, 1].TextString = "Tọa độ";
                acTable.Cells[2, 3].TextString = "Độ dài cạnh (m)";
                acTable.Cells[3, 1].TextString = "X";
                acTable.Cells[3, 2].TextString = "Y";

                //Set data
                BuildTable.AddPointsData(acTable, pts, pir.Value, lines);

                //Add table to model space
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acDb.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                acBlkTblRec.AppendEntity(acTable);
                acTrans.AddNewlyCreatedDBObject(acTable, true);
                acTrans.Commit();
            }

        }
    }
}
