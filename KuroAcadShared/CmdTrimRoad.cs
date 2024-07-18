using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(KuroAcad.CmdTrimRoad))]
namespace KuroAcad
{
    class CmdTrimRoad
    {
        [CommandMethod("KTrimRoad")]

        public static void TrimRoad()
        {
            // get the current document and database
            Database acCurDb;
            acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            var acDoc = Application.DocumentManager.MdiActiveDocument;

            //select list line
            List<Curve> listCur = new List<Curve>();
            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.MessageForAdding = "\nSelect Polyline to trim: ";
            pso.SingleOnly = false;

            PromptSelectionResult psr = acDoc.Editor.GetSelection(pso);
            if (psr.Status != PromptStatus.OK) return;

            //Start a transaction
            using (OpenCloseTransaction acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                //Open the block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                //open the block table record model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) 
                    as BlockTableRecord;

                //get list curves
                foreach (SelectedObject selectedObject in psr.Value)
                {
                    Entity ent = acTrans.GetObject(selectedObject.ObjectId, OpenMode.ForRead) as Entity;
                    if (ent is Curve)
                    {
                        Curve curve = ent as Curve;
                        listCur.Add(curve);
                    }
                }

                //check if the two curves is intersect
                for (int i = 0; i < listCur.Count; i++)
                {
                    for (int j = i + 1; j < listCur.Count; j++)
                    {
                        Curve curve1 = listCur[i];
                        Curve curve2 = listCur[j];

                        Point3dCollection pts = new Point3dCollection();
                        curve1.IntersectWith(curve2, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);

                        if (pts.Count > 0)
                        {
                            //add vertex to curve
                            foreach (Point3d pt in pts)
                            {
                                AddVertex(curve1, pt, acTrans);
                                AddVertex(curve2, pt, acTrans);
                            }
                        }
                    }
                }

                //commit transaction
                acTrans.Commit();
            }
        }
        internal static void AddVertex(Curve curve, Point3d point, OpenCloseTransaction trans)
        {
            Polyline pline = curve.ReplaceWithPolyline(trans);
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
    public static class ConvertToPolylineExtensions
    {
        /// <summary>
        /// Replaces the given database-resident Line or Arc
        /// with an equivalent polyline
        /// </summary>

        public static Polyline ReplaceWithPolyline(this Curve curve, OpenCloseTransaction trans)
        {
            return Convert(curve, trans);
        }

        public static Polyline ConvertToPolyline(this Curve curve)
        {
            return Convert(curve);
        }

        static Polyline Convert(Curve curve, OpenCloseTransaction trans = null)
        {
            Polyline pline = new Polyline(1);

            if (curve == null)
                throw new ArgumentNullException("curve");
            if (curve is Polyline)
            { 
                pline = (Polyline)curve;
            }

            if (curve.IsTransactionResident)
                throw new ArgumentException("curve must be from an OpenCloseTransaction");
            if (curve is Line || curve is Arc)
            {
                try
                {
                    Line line = null;
                    Arc arc = curve as Arc;

                    if (arc != null)
                    {
                        pline.Thickness = arc.Thickness;
                        pline.Normal = arc.Normal;
                    }
                    else
                    {
                        line = (Line)curve;
                        pline.Thickness = line.Thickness;

                        Vector3d normal = line.Normal;
                        Vector3d vector = line.StartPoint.GetVectorTo(line.EndPoint);
                        if (!vector.IsPerpendicularTo(normal))
                            normal = vector.GetPerpendicularVector();
                        pline.Normal = normal;
                    }

                    Point3d startPoint = curve.StartPoint.TransformBy(pline.Ecs.Inverse());
                    Point3d endPoint = curve.EndPoint.TransformBy(pline.Ecs.Inverse());
                    pline.Elevation = startPoint.Z;
                    pline.AddVertexAt(0, new Point2d(startPoint.X, startPoint.Y), 0, 0, 0);

                    if (line != null)
                        pline.AddVertexAt(1, new Point2d(endPoint.X, endPoint.Y), 0, 0, 0);
                    else
                        pline.JoinEntity(curve);
                    pline.SetPropertiesFrom(curve);
                    if (curve.Database != null && trans != null)
                    {
                        if (!curve.IsWriteEnabled)
                            curve.UpgradeOpen();
                        curve.HandOverTo(pline, true, true);
                        trans.AddNewlyCreatedDBObject(pline, true);
                        trans.AddNewlyCreatedDBObject(curve, false);
                        curve.Dispose();
                    }
                    return pline;
                }
                catch
                {
                    pline.Dispose();
                    throw;
                }
            }

            return pline;

        }
    }
}
