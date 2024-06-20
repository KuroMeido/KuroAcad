using Microsoft.Data.SqlClient;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace KuroAcad
{
    public class DBLoadUtil
    {
        //method to load all the Line Objects into Database
        public string LoadLines()
        {
            string result = " ";
            SqlConnection conn = DBUtil.GetConnection();

            try
            {
                //Get the Document and Editor object
                var doc = Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;

                //start a transaction
                using (Transaction trans = doc.TransactionManager.StartTransaction())
                {
                    TypedValue[] tv = new TypedValue[1];
                    tv.SetValue(new TypedValue((int)DxfCode.Start, "LINE"), 0);
                    SelectionFilter sf = new SelectionFilter(tv);

                    PromptSelectionResult promptSelectionResult = ed.SelectAll(sf);

                    //Check if there is object selected
                    if (promptSelectionResult.Status == PromptStatus.OK)
                    {
                        double startPtX = 0.0, startPtY = 0.0, startPtZ = 0.0;
                        string layer = " ", lineType = " ", color = " ";
                        double len = 0.0;
                        Line line = new Line();

                        SelectionSet ss = promptSelectionResult.Value;

                        String sql = @"INSERT INTO dbo.Lines (StartPtX, StartPtY, EndPtX, EndPtY, Layer, Color, LineType, Lenght, Color, Created)
                                        VALUES(@StartPtX, @StartPtY, @EndPtX, @EndPtY, @Layer, @Color, @LineType, @Lenght, @Color, @Created)";

                        conn.Open();

                        //Loop through the selected objects and insert into the database one line at a time
                        foreach (SelectedObject so in ss)
                        {
                            line = trans.GetObject(so.ObjectId, OpenMode.ForRead) as Line;

                            startPtX = line.StartPoint.X;
                            startPtY = line.StartPoint.Y;
                            startPtZ = line.StartPoint.Z;
                            layer = line.Layer;
                            lineType = line.Linetype;
                            color = line.Color.ColorName;
                            len = line.Length;

                            SqlCommand cmd = new SqlCommand(sql, conn);
                            cmd.Parameters.AddWithValue("@StartPtX", startPtX);
                            cmd.Parameters.AddWithValue("@StartPtY", startPtY);
                            cmd.Parameters.AddWithValue("@EndPtX", line.EndPoint.X);
                            cmd.Parameters.AddWithValue("@EndPtY", line.EndPoint.Y);
                            cmd.Parameters.AddWithValue("@Layer", layer);
                            cmd.Parameters.AddWithValue("@Color", color);
                            cmd.Parameters.AddWithValue("@LineType", lineType);
                            cmd.Parameters.AddWithValue("@Lenght", len);
                            cmd.Parameters.AddWithValue("@Created", DateTime.Now);
                            cmd.ExecuteNonQuery();

                        }
                    }
                    else
                    {
                        ed.WriteMessage("No object selected");
                    }
                }

            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            finally
            {
                if(conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return result;
        }
    }
}
