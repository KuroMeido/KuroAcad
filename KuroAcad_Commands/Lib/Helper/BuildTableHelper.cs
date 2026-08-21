namespace KuroAcad.Helper
{
    public static class BuildTableHelper
    {
        //method to add points data to table
        public static void AddPointsData(Table acTable, List<Point3d> pts, int numAfterDot, List<Line> lines)
        {
            int i = 3;
            foreach (Point3d pt in pts)
            {
                i++;
                acTable.InsertRows(i, 1, 1);
                acTable.Cells[i, 0].TextString = (i - 3).ToString();
                acTable.Cells[i, 1].TextString = pt.X.ToString("F" + numAfterDot);
                acTable.Cells[i, 2].TextString = pt.Y.ToString("F" + numAfterDot);
            }

            acTable.Cells[i, 0].TextString = "1";
            i = 3;

            foreach (Line line in lines)
            {
                i++;
                acTable.Cells[i, 3].TextString = line.Length.ToString("F" + numAfterDot);
            }
        }

        //method to lookup value in table
        public static int LookupValueInTable(Table acTable, string strKey)
        {
            int k = 0;
            for (int j = 0; j < acTable.Rows.Count; j++)
            {
                for (int i = 0; i < acTable.Columns.Count; i++)
                {
                    var cellValue = acTable.Cells[j, i].Value;
                    if (cellValue == null)
                    {
                        continue;
                    }

                    string str = cellValue.ToString();
                    int lastSemicolonIndex = str.LastIndexOf(';');
                    string output = str.Substring(lastSemicolonIndex + 1);

                    if (output == strKey)
                    {
                        k = i;
                    }
                }
            }

            int columnIndex = k;
            return columnIndex;
        }


    }
}