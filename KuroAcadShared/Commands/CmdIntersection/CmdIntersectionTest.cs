[assembly: CommandClass(typeof(KuroAcad.CmdIntersectionTest))]
namespace KuroAcad
{
    class CmdIntersectionTest
    {
        [CommandMethod("KIntersectionTest")]
        public static void IntersectionTest()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.
                DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            Editor ed = doc.Editor;

            //select list polyline
            PromptEntityOptions peo = new PromptEntityOptions("\nSelect a polyline: ");
            peo.SetRejectMessage("\nSelected object is not a polyline.");
            peo.AddAllowedClass(typeof(Polyline), true);
            PromptEntityResult per = ed.GetEntity(peo);
            if (per.Status != PromptStatus.OK) return;

            //Open the selected polyline for read
            Transaction tr = db.TransactionManager.StartTransaction();

            //start the transaction
            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                // Open the block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead)
                    as BlockTable;

                // Open the block table record model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                //Create a polyline
                Polyline acPoly = new Polyline();
                acPoly.SetDatabaseDefaults();
                acPoly.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
                acPoly.AddVertexAt(1, new Point2d(0, 50), 0, 0, 0);
                acPoly.AddVertexAt(2, new Point2d(75, 50), 0, 0, 0);

                Polyline acPoly2 = new Polyline();
                acPoly2.SetDatabaseDefaults();
                acPoly2.AddVertexAt(0, new Point2d(30, 30), 0, 0, 0);
                acPoly2.AddVertexAt(1, new Point2d(-15, 30), 0, 0, 0);

                acBlkTblRec.AppendEntity(acPoly);
                acTrans.AddNewlyCreatedDBObject(acPoly, true);

                acBlkTblRec.AppendEntity(acPoly2);
                acTrans.AddNewlyCreatedDBObject(acPoly2, true);

                //Intersection Points Collection
                Point3dCollection intersections = new Point3dCollection();
                acPoly.IntersectWith(acPoly2, Intersect.OnBothOperands,
                    intersections, IntPtr.Zero, IntPtr.Zero);

                ed.WriteMessage("\nNo. Of Intersection Points: {0}", intersections.Count);
                for (int i = 0; i < intersections.Count; i++)
                {
                    ed.WriteMessage("\n Point No. {0} X:{1} Y:{2}", i.ToString(), intersections[i].X, intersections[i].Y);
                }

                AddVertex(acPoly, intersections[0]);

                acTrans.Commit();
            }
        }

        private static void AddVertex(Polyline pline, Point3d point)
        {
            point = pline.GetClosestPointTo(point, false);        // point on curve
            double parameter = pline.GetParameterAtPoint(point);  // parameter at point
            int index = (int)parameter;                           // segment index

            // do not add a new vertex if point is on an existing one
            if (parameter == index) return;

            double bulge = pline.GetBulgeAt(index);               // segment bulge
            var plane = new Plane(Point3d.Origin, pline.Normal);  // polyline OCS plane


            if (bulge == 0.0) // linear segment
            {
                pline.AddVertexAt(index + 1, point.Convert2d(plane), 0.0, 0.0, 0.0);
            }
            else // arc segment
            {
                double angle = Math.Atan(bulge);              // quarter of total arc angle
                double angle1 = angle * (parameter - index);  // quarter of first arc angle
                double angle2 = angle - angle1;               // quarter of second arc angle

                // add the new vertex and set it bulge
                pline.AddVertexAt(index + 1, point.Convert2d(plane), Math.Tan(angle2), 0.0, 0.0);
                // set the bulge of the fist arc segment
                pline.SetBulgeAt(index, Math.Tan(angle1));
            }
        }
    }
}
