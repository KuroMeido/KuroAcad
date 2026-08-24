namespace KuroAcad.Helper
{
    internal static class BlockInsertHelper
    {
        internal static BlockReference InsertingABlock(Database db, Transaction acTrans, string blockName, Point3d originPt)
        {
            BlockTable acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            ObjectId blkRecId = ObjectId.Null;

            if (!acBlkTbl.Has(blockName))
            {
                using (BlockTableRecord acBlkTblRec = new BlockTableRecord())
                {
                    acBlkTblRec.Name = blockName;
                    acBlkTblRec.Origin = originPt;
                    blkRecId = acBlkTblRec.Id;
                }
            }
            else
            {
                blkRecId = acBlkTbl[blockName];
            }

            if (blkRecId != ObjectId.Null)
            {
                using (BlockReference acBlkRef = new BlockReference(new Point3d(0, 0, 0), blkRecId))
                {
                    BlockTableRecord acCurSpaceBlkTblRec = acTrans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                    acCurSpaceBlkTblRec.AppendEntity(acBlkRef);
                    acTrans.AddNewlyCreatedDBObject(acBlkRef, true);
                    return acBlkRef;
                }
            }

            return null;
        }

        internal static void CopyEntities(Database db, Transaction acTrans, Entity ent, Point3d pt)
        {
            using (Entity entCopy = ent.Id.GetObject(OpenMode.ForRead) as Entity)
            {
                if (entCopy != null)
                {
                    Entity entCopy1 = entCopy.Clone() as Entity;
                    entCopy1.TransformBy(Matrix3d.Displacement(pt.GetAsVector()));

                    BlockTableRecord acBlkTblRec = acTrans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                    acBlkTblRec.AppendEntity(entCopy1);
                    acTrans.AddNewlyCreatedDBObject(entCopy1, true);
                }
            }
        }
    }
}