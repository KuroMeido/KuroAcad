namespace KuroAcad.Helper
{
    public static class ConvertToPolylineHelper
    {
        public static Polyline ReplaceWithPolyline(this Curve curve, Transaction trans)
        {
            return Convert(curve, trans);
        }

        public static Polyline ConvertToPolyline(this Curve curve)
        {
            return Convert(curve);
        }

        private static Polyline Convert(Curve curve, Transaction trans = null)
        {
            var pline = new Polyline(1);

            if (curve == null)
            {
                throw new ArgumentNullException(nameof(curve));
            }

            if (curve is not Line && curve is not Arc)
            {
                return pline;
            }

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
                    {
                        normal = vector.GetPerpendicularVector();
                    }

                    pline.Normal = normal;
                }

                Point3d startPoint = curve.StartPoint.TransformBy(pline.Ecs.Inverse());
                Point3d endPoint = curve.EndPoint.TransformBy(pline.Ecs.Inverse());
                pline.Elevation = startPoint.Z;
                pline.AddVertexAt(0, new Point2d(startPoint.X, startPoint.Y), 0, 0, 0);

                if (line != null)
                {
                    pline.AddVertexAt(1, new Point2d(endPoint.X, endPoint.Y), 0, 0, 0);
                }
                else
                {
                    pline.JoinEntity(curve);
                }

                pline.SetPropertiesFrom(curve);

                if (curve.Database != null && trans != null)
                {
                    if (!curve.IsWriteEnabled)
                    {
                        curve.UpgradeOpen();
                    }

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
    }
}
