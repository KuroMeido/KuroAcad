using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using KuroAcad.Helper;

namespace KuroAcad
{
    internal static class TKLDUtil
    {
        internal static void TKLD()
        {
            // Get active Document and Database
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acDb = acDoc.Database;

            // Start transaction
            using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
            {
                // Create PromptSelectionOptions
                PromptSelectionOptions pso = new PromptSelectionOptions();
                pso.MessageForAdding = "\nSelect block references: ";
                pso.SingleOnly = false;

                // Get selection
                PromptSelectionResult psr = acDoc.Editor.GetSelection(pso);
                if (psr.Status != PromptStatus.OK || psr.Value.Count == 0)
                {
                    acDoc.Editor.WriteMessage("\nPlease select objects before running this command.");
                    return;
                }

                // Create Block Reference filter
                List<Entity> blRefEntities = new List<Entity>();
                foreach (SelectedObject so in psr.Value)
                {
                    Entity ent = (Entity)acTrans.GetObject(so.ObjectId, OpenMode.ForRead);
                    if (ent is BlockReference)
                    {
                        blRefEntities.Add(ent);
                    }
                }

                // Sort blocks by first character of attribute tag "TEN"
                List<Entity> blSorted = BlockAttributeHelper.SortBlocksByAttributeList(blRefEntities, acTrans);

                // Get point from user
                PromptPointResult ppr = acDoc.Editor.GetPoint("\nSelect an insertion point: ");
                if (ppr.Status != PromptStatus.OK)
                    return;

                Point3d pt = ppr.Value;

                // Create table
                Table acTable = new Table();
                acTable.TableStyle = acDoc.Database.Tablestyle;
                acTable.Position = pt;
                acTable.NumRows = 3;
                acTable.NumColumns = 8;

                // Set header
                acTable.Cells[1, 0].TextString = "NO.";
                acTable.Cells[1, 1].TextString = "LOT SYMBOL";
                acTable.Cells[1, 2].TextString = "QUANTITY";
                acTable.Cells[1, 3].TextString = "AREA EACH LOT (ha)";
                acTable.Cells[1, 4].TextString = "TOTAL AREA (ha)";
                acTable.Cells[1, 5].TextString = "MAX DENSITY";
                acTable.Cells[1, 6].TextString = "MAX FLOORS";
                acTable.Cells[1, 7].TextString = "FAR";

                // Set title
                acTable.Cells[0, 0].SetValue("LAND-USE STATISTICS TABLE", ParseOption.ParseOptionNone);

                // Set data
                List<char> listChar = BlockAttributeHelper.GetListFirstChar(blSorted, acTrans);
                foreach (char c in listChar)
                {
                    List<Entity> blByChar = BlockAttributeHelper.GetBlockAttributes(blSorted, acTrans, c);
                    BlockAttributeHelper.AddDataToTable(acTable, blByChar, acTrans, c);
                }

                // Total row
                acTable.InsertRows(acTable.Rows.Count, 1, 1);
                acTable.Cells[acTable.Rows.Count - 1, 0].TextString = "TOTAL";

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