

[assembly: CommandClass(typeof(KuroAcad.CmdTKLD))]

namespace KuroAcad
{
    internal class CmdTKLD
    {
        [CommandMethod("KuroTKLD")]

        public void KuroTKLD()
        {
            // Lấy document đang hoạt động
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acDb = acDoc.Database;

            // Bắt đầu transaction
            using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
            {
                // Tạo PromptSelectionOptions
                PromptSelectionOptions pso = new PromptSelectionOptions();
                pso.MessageForAdding = "\nChọn Block Reference: ";
                pso.SingleOnly = false;

                // Lấy danh sách block được chọn
                PromptSelectionResult psr = acDoc.Editor.GetSelection(pso);
                if (psr.Status != PromptStatus.OK || psr.Value.Count == 0)
                {
                    acDoc.Editor.WriteMessage("\nVui lòng chọn đối tượng trước khi thực hiện.");
                    return;
                }
                // Tạo bộ lọc chỉ lấy đối tượng Block Reference
                List<Entity> blRefEntities = new List<Entity>();
                foreach (SelectedObject so in psr.Value)
                {
                    Entity ent = (Entity)acTrans.GetObject(so.ObjectId, OpenMode.ForRead);
                    if (ent is BlockReference)
                    {
                        blRefEntities.Add(ent);
                    }
                }
                //Phân loại Block theo kí tự đầu tiên cuả Attribute Tag "TEN"
                List<Entity> blSorted = KuroExtensions.SortBlocksByAttributeList(blRefEntities, acTrans);

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

                //Tạo bảng thống kê
                Table acTable = new Table();
                acTable.TableStyle = acDoc.Database.Tablestyle;
                acTable.Position = pt;
                acTable.NumRows = blRefEntities.Count + 4 ;
                acTable.NumColumns = 7;



                // Tính toán và điền dữ liệu vào bảng
                int rowIndex = 2;
                int inDex = 1;
                foreach (Entity ent in blSorted)
                {
                    BlockReference blRef = ent as BlockReference;
                    if (blRef != null)
                    {
                        // Lấy giá trị các attribute của block
                        AttributeCollection acAttColl = blRef.AttributeCollection;
                        string tenLoDat = "";
                        double dienTich = 0;
                        double mDXDmax = 0;
                        int tangCaoMax = 0;
                        int soLuong = 1;

                        // Duyệt qua các attribute và lấy giá trị
                        foreach (ObjectId acAttId in acAttColl)
                        {
                            using (AttributeReference acAtt = (AttributeReference)acTrans.GetObject(acAttId, OpenMode.ForRead))
                            {
                                if (acAtt.Tag == "A")
                                {
                                    tenLoDat = acAtt.TextString;
                                }
                                else if (acAtt.Tag.ToString() == "8053,44")
                                {
                                    dienTich = double.Parse(acAtt.TextString)/10000;
                                }
                                else if (acAtt.Tag.ToString() == "70,0")
                                {
                                    mDXDmax = double.Parse(acAtt.TextString)/10;
                                }
                                else if (acAtt.Tag.ToString() == "5")
                                {
                                    tangCaoMax = int.Parse(acAtt.TextString);
                                }
                            }
                        }
                        double hssDd = mDXDmax/100*tangCaoMax;
                        // Điền dữ liệu vào bảng
                        acTable.Cells[rowIndex, 0].TextString = inDex.ToString();
                        acTable.Cells[rowIndex, 1].TextString = tenLoDat;
                        acTable.Cells[rowIndex, 2].TextString = soLuong.ToString();
                        acTable.Cells[rowIndex, 3].TextString = dienTich.ToString("F2");
                        acTable.Cells[rowIndex, 4].TextString = mDXDmax.ToString();
                        acTable.Cells[rowIndex, 5].TextString = tangCaoMax.ToString();
                        acTable.Cells[rowIndex, 6].TextString = hssDd.ToString("F2");

                        rowIndex++;
                        inDex = inDex + 1;
                    }
                }

                // Sắp xếp bảng dữ liệu theo bảng chữ cái của cột "TEN LO DAT"

                //Thiết lập header của bảng
                acTable.Cells[1, 0].TextString = "STT";
                acTable.Cells[1, 1].TextString = "KÍ HIỆU LÔ ĐẤT";
                acTable.Cells[1, 2].TextString = "SỐ LƯỢNG";
                acTable.Cells[1, 3].TextString = "DIỆN TÍCH (ha)";
                acTable.Cells[1, 4].TextString = "MĐXD max";
                acTable.Cells[1, 5].TextString = "TẦNG CAO max";
                acTable.Cells[1, 6].TextString = "HSSDĐ ";


                // Thiết lập tiêu đề cho bảng
                acTable.Cells[0, 0].SetValue("BẢNG THỐNG KÊ LÔ ĐẤT", ParseOption.ParseOptionNone);

                //thiết lập hàng tổng diện tích
                acTable.Cells[acTable.NumRows - 1, 0].TextString = "TỔNG CỘNG";
                acTable.Cells[acTable.NumRows - 1, 2].TextString = (inDex-1).ToString();
                acTable.Cells[acTable.NumRows - 1, 3].TextString = totalArea.ToString();

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
