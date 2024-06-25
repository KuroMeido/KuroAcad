using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
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
            //Get the Document and Editor object
            var doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            var data = new DatabaseManager();

            try
            {
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

                        String sql = @"INSERT INTO dbo.Lines (StartPtX, StartPtY, EndPtX, EndPtY, Layer, Color, LineType, Length, Created)
                                            VALUES(@StartPtX, @StartPtY, @EndPtX, @EndPtY, @Layer, @Color, @LineType, @Length, @Created)";

                        //Loop through the selected objects and insert into the database one line at a time
                        foreach (SelectedObject so in ss)
                        {
                            line = trans.GetObject(so.ObjectId, OpenMode.ForRead) as Line;

                            if (line != null)
                            {
                                startPtX = line.StartPoint.X;
                                startPtY = line.StartPoint.Y;
                                startPtZ = line.StartPoint.Z;
                                layer = line.Layer;
                                lineType = line.Linetype;
                                color = line.Color.ColorName;
                                len = line.Length;

                                using (SqlCommand cmd = new SqlCommand(sql, data.GetConnection()))
                                {
                                    cmd.Parameters.AddWithValue("@StartPtX", startPtX);
                                    cmd.Parameters.AddWithValue("@StartPtY", startPtY);
                                    cmd.Parameters.AddWithValue("@EndPtX", line.EndPoint.X);
                                    cmd.Parameters.AddWithValue("@EndPtY", line.EndPoint.Y);
                                    cmd.Parameters.AddWithValue("@Layer", layer);
                                    cmd.Parameters.AddWithValue("@Color", color);
                                    cmd.Parameters.AddWithValue("@LineType", lineType);
                                    cmd.Parameters.AddWithValue("@Length", len);
                                    cmd.Parameters.AddWithValue("@Created", DateTime.Now);
                                    cmd.ExecuteNonQuery();
                                }
                            }
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
                data.Dispose();
            }
            return result;
        }
    }

    public class DatabaseManager : IDisposable
    {
        private readonly string? _connectionString;
        private bool disposedValue;

        public DatabaseManager()
        {
            // Target this class for user secrets
            var builder = new ConfigurationBuilder()
                             .AddUserSecrets<DatabaseManager>();

            IConfiguration configuration = builder.Build();
            _connectionString = configuration.GetConnectionString("mssqlserver");
        }

        public SqlConnection GetConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //nothing to dispose, connection is disposed in using block
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
