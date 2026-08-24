namespace KuroAcad.Helper
{
    internal static class GeometryHelper
    {
        internal static Point3d GetCenterPoint(Polyline pl)
        {
            return new Point3d(
                (pl.GeometricExtents.MinPoint.X + pl.GeometricExtents.MaxPoint.X) / 2,
                (pl.GeometricExtents.MinPoint.Y + pl.GeometricExtents.MaxPoint.Y) / 2,
                0);
        }
    }
}