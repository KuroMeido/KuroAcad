namespace KuroAcad.Helper
{
    internal static class LayerHelper
    {
        internal static Boolean IsExistingLayer(Transaction trans, LayerTable ltb, string layerName)
        {
            foreach (ObjectId id in ltb)
            {
                LayerTableRecord lr = trans.GetObject(id, OpenMode.ForRead) as LayerTableRecord;
                if (lr.Name == layerName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}