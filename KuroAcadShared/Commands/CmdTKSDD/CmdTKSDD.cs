using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(KuroAcad.CmdTKSDD))]

namespace KuroAcad
{
    class CmdTKSDD
    {
        [CommandMethod("KTKSDD")]
        public void KuroTKSDD()
        {
            // Lấy document đang hoạt động
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acDb = acDoc.Database;

            // Bắt đầu transaction
            using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
            {
                // Tạo PromptSelectionOptions
                PromptSelectionOptions pso = new PromptSelectionOptions();
                pso.MessageForAdding = "\nChọn các đối tượng Hatch: ";
                pso.SingleOnly = false;

                // Lấy danh sách hatch được chọn
                PromptSelectionResult psr = acDoc.Editor.GetSelection(pso);
                if (psr.Status != PromptStatus.OK || psr.Value.Count == 0)
                {
                    acDoc.Editor.WriteMessage("\nVui lòng chọn các đối tượng Hatch trước khi thực hiện.");
                    return;
                }
                // Tạo bộ lọc chỉ lấy đối tượng hatch
                List<Entity> hatchEntities = new List<Entity>();
                foreach (SelectedObject so in psr.Value)
                {
                    Entity ent = (Entity)acTrans.GetObject(so.ObjectId, OpenMode.ForRead);
                    if (ent is Hatch)
                    {
                        hatchEntities.Add(ent);
                    }
                }

                // Phân loại hatch theo layer
                Dictionary<string, List<Entity>> hatchByLayer = new Dictionary<string, List<Entity>>();
                foreach (Entity ent in hatchEntities)
                {
                    string layerName = ent.Layer;
                    if (!hatchByLayer.ContainsKey(layerName))
                    {
                        hatchByLayer[layerName] = new List<Entity>();
                    }
                    hatchByLayer[layerName].Add(ent);
                }

                // Lấy điểm người dùng chọn
                PromptPointResult ppr = acDoc.Editor.GetPoint("\nChọn một điểm bất kỳ trên bản vẽ: ");
                if (ppr.Status != PromptStatus.OK)
                    return;
                Point3d pt = ppr.Value;

                // Lấy giá trị TotalArea từ người dùng
                PromptDoubleOptions pdo = new PromptDoubleOptions("\nNhập tổng diện tích (ha): ");
                PromptDoubleResult pdr = acDoc.Editor.GetDouble(pdo);
                if (pdr.Status != PromptStatus.OK)
                    return;
                double totalArea = pdr.Value;


                // Tạo bảng thống kê
                Table acTable = new Table();
                acTable.TableStyle = acDoc.Database.Tablestyle;
                acTable.Position = pt;
                acTable.NumRows = hatchByLayer.Count + 4;
                acTable.NumColumns = 4;

                // Thiết lập header của bảng
                acTable.Cells[1, 0].TextString = "STT";
                acTable.Cells[1, 1].TextString = "CHUC NANG SU DUNG DAT";
                acTable.Cells[1, 2].TextString = "DIEN TICH (HA)";
                acTable.Cells[1, 3].TextString = "TY LE (%)";

                // Tính toán và điền dữ liệu vào bảng
                int rowIndex = 2;
                int inDex = 1;
                double totalAreaHatch = 0;

                foreach (var kvp in hatchByLayer)
                {
                    string layerName = kvp.Key;
                    List<Entity> hatchList = kvp.Value;
                    double layerArea = 0;
                    foreach (Entity ent in hatchList)
                    {
                        Hatch hatch = (Hatch)ent;
                        try
                        {
                            layerArea += hatch.Area / 10000;
                        }
                        catch
                        {
                            //change hatch color
                            hatch.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0);
                        }
                    }

                    acTable.Cells[rowIndex, 0].TextString = KuroExtensions.convertToRoman(inDex);
                    acTable.Cells[rowIndex, 1].TextString = layerName;
                    acTable.Cells[rowIndex, 2].TextString = layerArea.ToString("F2");
                    rowIndex++;
                    inDex = inDex + 1;
                    totalAreaHatch += layerArea;
                }
                rowIndex = 2;
                foreach (var kvp in hatchByLayer)
                {
                    string layerName = kvp.Key;
                    List<Entity> hatchList = kvp.Value;
                    double layerArea = 0;
                    foreach (Entity ent in hatchList)
                    {
                        Hatch hatch = (Hatch)ent;
                        try
                        {
                            layerArea += hatch.Area / 10000;
                        }
                        catch
                        {

                        }
                    }
                    acTable.Cells[rowIndex, 3].TextString = ((layerArea / totalArea) * 100).ToString("F2") + "%";
                    rowIndex++;
                }

                // Thiết lập tiêu đề cho bảng
                acTable.Cells[0, 0].SetValue("BANG CO CAU SU DUNG DAT", ParseOption.ParseOptionNone);

                //THIẾT LẬP HÀNG ĐẤT GIAO THÔNG
                acTable.Cells[acTable.NumRows - 2, 0].TextString = KuroExtensions.convertToRoman((acTable.NumRows - 3));
                acTable.Cells[acTable.NumRows - 2, 1].TextString = "DAT GIAO THONG";
                acTable.Cells[acTable.NumRows - 2, 2].TextString = (totalArea - totalAreaHatch).ToString("F2");
                acTable.Cells[acTable.NumRows - 2, 3].TextString = (((totalArea - totalAreaHatch) / totalArea) * 100).ToString("F2") + "%";

                //thiết lập hàng tổng diện tích
                acTable.Cells[acTable.NumRows - 1, 0].TextString = "TONG CONG";
                acTable.Cells[acTable.NumRows - 1, 2].TextString = totalArea.ToString();
                acTable.Cells[acTable.NumRows - 1, 3].TextString = "100%";

                // Thêm bảng vào không gian hiện tại
                BlockTableRecord btr = (BlockTableRecord)acTrans.GetObject(acDb.CurrentSpaceId, OpenMode.ForWrite);
                btr.AppendEntity(acTable);
                acTrans.AddNewlyCreatedDBObject(acTable, true);

                // Commit transaction
                acTrans.Commit();
            }

        }
    }
}