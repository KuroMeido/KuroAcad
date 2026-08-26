using Autodesk.AutoCAD.Colors;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Color = Autodesk.AutoCAD.Colors.Color;
using KuroAcad.Helper;

[assembly: CommandClass(typeof(KuroAcad.CmdRoad))]
namespace KuroAcad
{
    class CmdRoad
    {
        [CommandMethod("KRoad")]
        public void DrawRoad()
        {
            // Get the current document and database
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            Editor ed = doc.Editor;
            ObjectId layerId = db.LayerTableId;

            //Get multiple selected polyline from user
            #region get selected polylines List<ObjectId> plineIdsFiltered
            var peOpt = new PromptEntityOptions("\nSelect polylines: ");
            peOpt.AllowNone = false;
            peOpt.SetRejectMessage("\nSelected object is not a polyline.");
            peOpt.AddAllowedClass(typeof(Polyline), true);

            // The next lines are to allow for other objects
            // THESE ARE THE OBJECTS IM TRYING TO ADD
            peOpt.AddAllowedClass(typeof(Line), false);
            peOpt.AddAllowedClass(typeof(Polyline2d), false);
            peOpt.AddAllowedClass(typeof(Spline), false);

            PromptEntityResult peRes = ed.GetEntity(peOpt);

            #endregion

            //Start a transaction
            Transaction trans = db.TransactionManager.StartTransaction();

            //Create a new layer for the road
            #region Create new layer
            LayerTable Layertb = trans.GetObject(layerId, OpenMode.ForWrite) as LayerTable;
            LayerTableRecord ltr = new LayerTableRecord();

            string layerName1 = "DUONG";
            if (LayerHelper.IsExistingLayer(trans, Layertb, layerName1) == false)
            {
                ltr.Name = layerName1;
                ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 255);
                Layertb.Add(ltr);
                trans.AddNewlyCreatedDBObject(ltr, true);
            }

            ltr = new LayerTableRecord(); //Add this line
            string layerName2 = "VIAHE";
            if (LayerHelper.IsExistingLayer(trans, Layertb, layerName2) == false)
            {
                ltr.Name = layerName2;
                ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 5);
                Layertb.Add(ltr);
                trans.AddNewlyCreatedDBObject(ltr, true);
            }
            trans.Commit();
            #endregion

            //Get number offset from user
            #region Get number offset layer2 double offset2
            var pio2 = new PromptDoubleOptions("\noffset Lo gioi: ");
            pio2.AllowNegative = false;
            pio2.AllowZero = false;
            pio2.DefaultValue = 6;
            var pir2 = ed.GetDouble(pio2);
            if (pir2.Status != PromptStatus.OK)
                return;
            double offset2 = pir2.Value;
            #endregion

            //Get number offset from user
            #region Get number offset layer1 double offset1
            var pio1 = new PromptDoubleOptions("\noffset Via he: ");
            pio1.AllowNegative = false;
            pio1.AllowZero = false;
            pio1.DefaultValue = 3;
            var pir1 = ed.GetDouble(pio1);
            if (pir1.Status != PromptStatus.OK)
                return;
            double offset1 = offset2 - pir1.Value;
            #endregion

            //Start a new transaction
            Transaction trans2 = db.TransactionManager.StartTransaction();

            //Create road
            #region Create road
            if (peRes.Status == PromptStatus.OK)
            {
                Entity selEnt =
                    (Entity)peRes.ObjectId.GetObject(OpenMode.ForRead);
                BlockTable bt =
                    (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
                BlockTableRecord btr =
                    (BlockTableRecord)trans2.GetObject(
                        db.CurrentSpaceId, OpenMode.ForWrite);

                var acLine = selEnt as Curve;

                #region Offset via he, duong
                //Offset the object in the first direction
                foreach (Entity acEnt in acLine.GetOffsetCurves(offset1))
                {
                    acEnt.Layer = layerName1;
                    acEnt.Linetype = "CONTINUOUS";
                    btr.AppendEntity(acEnt);
                    trans2.AddNewlyCreatedDBObject(acEnt, true);
                }
                // Now offset the object in the second direction
                foreach (Entity acEnt in acLine.GetOffsetCurves(-offset1))
                {
                    // Add each offset object
                    acEnt.Layer = layerName1;
                    acEnt.Linetype = "CONTINUOUS";
                    btr.AppendEntity(acEnt);
                    trans2.AddNewlyCreatedDBObject(acEnt, true);
                }
                //Offset the object in the first direction
                foreach (Entity acEnt in acLine.GetOffsetCurves(offset2))
                {
                    acEnt.Layer = layerName2;
                    acEnt.Linetype = "CONTINUOUS";
                    btr.AppendEntity(acEnt);
                    trans2.AddNewlyCreatedDBObject(acEnt, true);
                }
                // Now offset the object in the second direction
                foreach (Entity acEnt in acLine.GetOffsetCurves(-offset2))
                {
                    // Add each offset object
                    acEnt.Layer = layerName2;
                    acEnt.Linetype = "CONTINUOUS";
                    btr.AppendEntity(acEnt);
                    trans2.AddNewlyCreatedDBObject(acEnt, true);
                }
                #endregion
            }
            trans2.Commit();
            #endregion
        }
    }
}
