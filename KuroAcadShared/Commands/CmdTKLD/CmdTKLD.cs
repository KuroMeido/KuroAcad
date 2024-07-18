

using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(KuroAcad.CmdTKLD))]

namespace KuroAcad
{
    public class CmdTKLD
    {
        [CommandMethod("KTKLD")]

        public void KuroTKLD()
        {
            // Get active Document and Database
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acDb = acDoc.Database;

            // Start transaction
            using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
            {
                // Create PromptSelectionOptions
                PromptSelectionOptions pso = new PromptSelectionOptions();
                pso.MessageForAdding = "\nChọn Block Reference: ";
                pso.SingleOnly = false;

                // Get selection
                PromptSelectionResult psr = acDoc.Editor.GetSelection(pso);
                if (psr.Status != PromptStatus.OK || psr.Value.Count == 0)
                {
                    acDoc.Editor.WriteMessage("\nVui lòng chọn đối tượng trước khi thực hiện.");
                    return;
                }

                // Create Block Reference fitler
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

                // Get point from user
                PromptPointResult ppr = acDoc.Editor.GetPoint("\nChọn một điểm bất kỳ trên bản vẽ: ");
                if (ppr.Status != PromptStatus.OK)
                    return;
                Point3d pt = ppr.Value;

                //Create Table
                #region Create Table
                Table acTable = new Table();
                acTable.TableStyle = acDoc.Database.Tablestyle;
                acTable.Position = pt;
                acTable.NumRows = 3 ;
                acTable.NumColumns = 8;

                    //Set header
                    #region Set header
                    acTable.Cells[1, 0].TextString = "STT";
                    acTable.Cells[1, 1].TextString = "KÍ HIỆU LÔ ĐẤT";
                    acTable.Cells[1, 2].TextString = "SỐ LƯỢNG";
                    acTable.Cells[1, 3].TextString = "DIỆN TÍCH MỖI LÔ (ha)";
                    acTable.Cells[1, 4].TextString = "DIỆN TÍCH TỔNG (ha)";
                    acTable.Cells[1, 5].TextString = "MĐXD max";
                    acTable.Cells[1, 6].TextString = "TẦNG CAO max";
                    acTable.Cells[1, 7].TextString = "HSSDĐ ";
                    #endregion

                    // set title
                    #region Set title
                    acTable.Cells[0, 0].SetValue("BẢNG THỐNG KÊ LÔ ĐẤT", ParseOption.ParseOptionNone);
                    #endregion

                    //set data
                    #region Set data
                    //get list of first character of attribute value
                    List<char> listChar = KuroExtensions.GetListFirstChar(blSorted, acTrans);
                    foreach (char c in listChar)
                    {
                        //get list of block reference by first character of attribute value
                        List<Entity> blByChar = KuroExtensions.GetBlockAttributes(blSorted, acTrans, c);
                        //add data to table
                        KuroExtensions.AddDataToTable(acTable, blByChar, acTrans, c);
                    }


                    //total area
                    acTable.InsertRows(acTable.Rows.Count, 1, 1);
                    acTable.Cells[acTable.Rows.Count -1, 0].TextString = "TỔNG CỘNG";
                    #endregion

                #endregion
                // Add table to current space
                BlockTableRecord btr = (BlockTableRecord)acTrans.GetObject(acDb.CurrentSpaceId, OpenMode.ForWrite);
                btr.AppendEntity(acTable);
                acTrans.AddNewlyCreatedDBObject(acTable, true);

                // Commit transaction
                acTrans.Commit();
            }
        }
    }
}
