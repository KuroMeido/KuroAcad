
//using Autodesk.AutoCAD.ApplicationServices;

//namespace KuroAcad2025.Extensions
//{
//    class Extensions
//    {
//        public static Table BuildTable(string title, List<string> headers, Point3d insertPoint, MyDataCollection tableData, Document doc)
//        {
//            /*
//                 *Table gets created with default 1 row and 1 column
//                 * so when inserting default rows and columns, remove old row and column
//                 * which get pushed to the last index
//                 */

//            Table tb = new Table();

//            try
//            {
//                tb.Position = insertPoint;
//                tb.Layer = LayerName;
//                tb.TableStyle = doc.Database.Tablestyle;



//                int titleRow = 0, headersRow = 1, totalRow = 2;


//                tb.InsertColumns(0, 1.5, 4);
//                tb.DeleteColumns(tb.Columns.Count - 1, 1);//deletes the default column
//                tb.SetColumnWidth(1.5);
//                tb.Columns[1].Width = 1.63;
//                tb.Columns[2].Width = 1;


//                tb.InsertRows(0, .5, 3);
//                tb.DeleteRows(tb.Rows.Count - 1, 1);
//                tb.SetRowHeight(.5);
//                tb.Cells.TextHeight = .19;//main text height

//                //Title row
//                CellRange titleRange = CellRange.Create(tb, titleRow, 0, titleRow, 3);
//                tb.MergeCells(titleRange);
//                tb.Rows[titleRow].TextHeight = .25;//bigger title
//                tb.Cells[titleRow, 0].TextString = title;

//                tb.Rows[titleRow].Alignment = CellAlignment.MiddleCenter;

//                //headers row
//                tb.Cells[headersRow, zoneCol].TextString = "FIRST";
//                tb.Cells[headersRow, manifoldCol].TextString = "SECOND";
//                tb.Cells[headersRow, loopCol].TextString = "THIRD";
//                tb.Cells[headersRow, lengthCol].TextString = "FOURTH";

//                tb.Rows[headersRow].Alignment = CellAlignment.MiddleCenter;


//                foreach (MyData data in tableData)
//                {
//                    int row = tb.Rows.Count - 1;
//                    tb.InsertRows(row, .38, 1);

//                    tb.Cells[row, 0].SetValue(data.First, ParseOption.SetDefaultFormat);

//                    tb.Cells[row, 1].SetValue(data.Second, ParseOption.SetDefaultFormat);

//                    tb.Cells[row, 2].SetValue(data.Third, ParseOption.SetDefaultFormat);

//                    tb.Cells[row, 3].TextString = data.Fourth;

//                }

//                //total row
//                int bottomRow = tb.Rows.Count - 1;

//                tb.Cells[bottomRow, manifoldCol].TextString = "TOTAL";

//                tb.Cells[tb.Rows.Count - 1, 3].Contents.Add();
//                tb.Cells[tb.Rows.Count - 1, 3].Contents[0].Formula = "=Sum(D3:D" + (tb.Rows.Count - 1) + ")";


//                tb.GenerateLayout();
//            }
//            catch (Autodesk.AutoCAD.Runtime.Exception ex)
//            {
//                Active.WriteMessage($"\nError in {nameof(BuildTable)}: {ex.Message}");
//                return null;
//            }
//            return tb;
//        }

//    }
//}
