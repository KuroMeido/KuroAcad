using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace KuroAcad
{
    internal static class TKSDDUtil
    {
        internal static void TKSDD()
        {
            // Get active document and database
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acDb = acDoc.Database;

            // Start transaction
            using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
            {
                // Create selection options
                PromptSelectionOptions pso = new PromptSelectionOptions();
                pso.MessageForAdding = "\nSelect hatch objects: ";
                pso.SingleOnly = false;

                // Get selected objects
                PromptSelectionResult psr = acDoc.Editor.GetSelection(pso);
                if (psr.Status != PromptStatus.OK || psr.Value.Count == 0)
                {
                    acDoc.Editor.WriteMessage("\nPlease select hatch objects before running this command.");
                    return;
                }

                // Filter hatch entities only
                List<Entity> hatchEntities = new List<Entity>();
                foreach (SelectedObject so in psr.Value)
                {
                    Entity ent = (Entity)acTrans.GetObject(so.ObjectId, OpenMode.ForRead);
                    if (ent is Hatch)
                    {
                        hatchEntities.Add(ent);
                    }
                }

                // Group hatch entities by layer
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

                // Get insertion point
                PromptPointResult ppr = acDoc.Editor.GetPoint("\nSelect an insertion point: ");
                if (ppr.Status != PromptStatus.OK)
                    return;

                Point3d pt = ppr.Value;

                // Get total area
                PromptDoubleOptions pdo = new PromptDoubleOptions("\nEnter total area (ha): ");
                PromptDoubleResult pdr = acDoc.Editor.GetDouble(pdo);
                if (pdr.Status != PromptStatus.OK)
                    return;

                double totalArea = pdr.Value;

                // Create table
                Table acTable = new Table();
                acTable.TableStyle = acDoc.Database.Tablestyle;
                acTable.Position = pt;
                acTable.NumRows = hatchByLayer.Count + 4;
                acTable.NumColumns = 4;

                // Set header
                acTable.Cells[1, 0].TextString = "NO.";
                acTable.Cells[1, 1].TextString = "LAND USE FUNCTION";
                acTable.Cells[1, 2].TextString = "AREA (HA)";
                acTable.Cells[1, 3].TextString = "RATIO (%)";

                // Fill table data
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

                // Set table title
                acTable.Cells[0, 0].SetValue("LAND USE STRUCTURE TABLE", ParseOption.ParseOptionNone);

                // Set traffic land row
                acTable.Cells[acTable.NumRows - 2, 0].TextString = KuroExtensions.convertToRoman(acTable.NumRows - 3);
                acTable.Cells[acTable.NumRows - 2, 1].TextString = "TRAFFIC LAND";
                acTable.Cells[acTable.NumRows - 2, 2].TextString = (totalArea - totalAreaHatch).ToString("F2");
                acTable.Cells[acTable.NumRows - 2, 3].TextString = (((totalArea - totalAreaHatch) / totalArea) * 100).ToString("F2") + "%";

                // Set total row
                acTable.Cells[acTable.NumRows - 1, 0].TextString = "TOTAL";
                acTable.Cells[acTable.NumRows - 1, 2].TextString = totalArea.ToString();
                acTable.Cells[acTable.NumRows - 1, 3].TextString = "100%";

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