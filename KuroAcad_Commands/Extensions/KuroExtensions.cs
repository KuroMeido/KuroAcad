using System.Text;

namespace KuroAcad
{
    internal class KuroExtensions
    {
        internal static string convertToRoman(int number)
        {
            string[] romanNumerals = { "I", "IV", "V", "IX", "X", "XL", "L", "XC", "C", "CD", "D", "CM", "M" };
            int[] values = { 1, 4, 5, 9, 10, 40, 50, 90, 100, 400, 500, 900, 1000 };

            StringBuilder sb = new StringBuilder();

            for (int i = values.Length - 1; i >= 0; i--)
            {
                while (number >= values[i])
                {
                    sb.Append(romanNumerals[i]);
                    number -= values[i];
                }
            }

            return sb.ToString();
        }

        internal static void SortStringList(List<string> stringList)
        {
            stringList.Sort((a, b) =>
            {
                int minLength = Math.Min(a.Length, b.Length);
                for (int i = 0; i < minLength; i++)
                {
                    if (a[i] != b[i])
                    {
                        return a[i].CompareTo(b[i]);
                    }
                }
                // Nếu tất cả các ký tự giống nhau, so sánh độ dài của chuỗi
                return a.Length.CompareTo(b.Length);
            });
        }

        internal static List<Entity> SortBlocksByAttributeList(List<Entity> blocks, Transaction acTrans)
        {
            Dictionary<Entity, string> blockSort = new Dictionary<Entity, string>();
            List<string> listAttributes = new List<string>();
            foreach (Entity block in blocks)
            {
                string blockName = "";
                BlockReference blRef = block as BlockReference;
                if (blRef != null)
                {
                    AttributeCollection acAttColl = blRef.AttributeCollection;


                    foreach (ObjectId acAttId in acAttColl)
                    {
                        using (AttributeReference acAtt = (AttributeReference)acTrans.GetObject(acAttId, OpenMode.ForRead))
                        {
                            if (acAtt.Tag == "A")
                            {
                                blockName = acAtt.TextString;
                            }
                        }
                    }
                }
                blockSort.Add(block, blockName);
                listAttributes.Add(blockName);
            }
            SortStringList(listAttributes);

            // Tạo danh sách KeyValuePair từ blockSort
            List<KeyValuePair<Entity, string>> sortedBlockSort = new List<KeyValuePair<Entity, string>>(blockSort);

            // Sắp xếp danh sách KeyValuePair dựa trên giá trị của listAttributes
            sortedBlockSort.Sort((a, b) => listAttributes.IndexOf(a.Value).CompareTo(listAttributes.IndexOf(b.Value)));

            // Tạo danh sách kết quả
            List<Entity> sortedBlocks = new List<Entity>();
            foreach (var item in sortedBlockSort)
            {
                sortedBlocks.Add(item.Key);
            }

            return sortedBlocks;
        }

        // Get list of BlockReference by first character of attribute value
        internal static List<Entity> GetBlockAttributes(List<Entity> blocks, Transaction acTrans, char strKey)
        {
            List<Entity> blByChar = new List<Entity>();
            List<string> attributes = new List<string>();
            foreach (Entity block in blocks)
            {
                BlockReference blRef = block as BlockReference;
                if (blRef != null)
                {
                    AttributeCollection acAttColl = blRef.AttributeCollection;

                    foreach (ObjectId acAttId in acAttColl)
                    {
                        using (AttributeReference acAtt = (AttributeReference)acTrans.GetObject(acAttId, OpenMode.ForRead))
                        {
                            if (acAtt.Tag == "TEN")
                            {
                                if (acAtt.TextString[0] == strKey)
                                {
                                    blByChar.Add(block);
                                }
                            }
                        }

                    }
                }
            }
            return blByChar;
        }

        //method to get list first character of attribute value
        internal static List<char> GetListFirstChar(List<Entity> blocks, Transaction acTrans)
        {
            List<char> listChar = new List<char>();
            foreach (Entity block in blocks)
            {
                BlockReference blRef = block as BlockReference;
                if (blRef != null)
                {
                    AttributeCollection acAttColl = blRef.AttributeCollection;

                    foreach (ObjectId acAttId in acAttColl)
                    {
                        using (AttributeReference acAtt = (AttributeReference)acTrans.GetObject(acAttId, OpenMode.ForRead))
                        {
                            if (acAtt.Tag == "TEN")
                            {
                                if (!listChar.Contains(acAtt.TextString[0]))
                                {
                                    listChar.Add(acAtt.TextString[0]);
                                }
                            }
                        }
                    }
                }
            }
            return listChar;
        }

        //method to add data to exist table
        internal static void AddDataToTable(Table acTable, List<Entity> blocks, Transaction acTrans, char strKey)
        {
            //Count List of BlockReference
            int count = blocks.Count;
            double totalArea = 0;
            //Add row for table
            acTable.InsertRows(acTable.Rows.Count, 1, count + 1);

            // set data for each block
            int rowIndex = acTable.Rows.Count - count - 1;
            int inDex = 1;

            string blName = "";
            double blSquare = 0;
            double mDXDmax = 0;
            int tangCaoMax = 0;
            int soLuong = 1;

            foreach (Entity block in blocks)
            {
                BlockReference blRef = block as BlockReference;

                // Get attribute value
                AttributeCollection acAttColl = blRef.AttributeCollection;

                //Set attribute value
                foreach (ObjectId acAttId in acAttColl)
                {
                    using (AttributeReference acAtt = (AttributeReference)acTrans.GetObject(acAttId, OpenMode.ForRead))
                    {
                        if (acAtt.Tag == "TEN")
                        {
                            blName = acAtt.TextString;
                        }
                        else if (acAtt.Tag.ToString() == "DT")
                        {
                            blSquare = double.Parse(acAtt.TextString) / 10000;
                        }
                        else if (acAtt.Tag.ToString() == "MDXD")
                        {
                            mDXDmax = double.Parse(acAtt.TextString) / 10;
                        }
                        else if (acAtt.Tag.ToString() == "TC")
                        {
                            tangCaoMax = int.Parse(acAtt.TextString);
                        }
                    }
                }
                double hssDd = mDXDmax / 100 * tangCaoMax;
                totalArea = totalArea + blSquare;

                // fill data to table
                acTable.Cells[rowIndex, 0].TextString = inDex.ToString();
                acTable.Cells[rowIndex, 1].TextString = blName;
                acTable.Cells[rowIndex, 2].TextString = soLuong.ToString();
                acTable.Cells[rowIndex, 3].TextString = blSquare.ToString("F2");
                acTable.Cells[rowIndex, 5].TextString = mDXDmax.ToString();
                acTable.Cells[rowIndex, 6].TextString = tangCaoMax.ToString();
                acTable.Cells[rowIndex, 7].TextString = hssDd.ToString("F2");

                rowIndex++;
                inDex = inDex + 1;
            }

            // set header of list blocks
            acTable.Cells[acTable.Rows.Count - count - 2, 1].TextString = strKey.ToString();
            acTable.Cells[acTable.Rows.Count - count - 2, 2].TextString = count.ToString();
            acTable.Cells[acTable.Rows.Count - count - 2, 4].TextString = totalArea.ToString("F2");
            acTable.Cells[acTable.Rows.Count - count - 2, 5].TextString = mDXDmax.ToString();
            acTable.Cells[acTable.Rows.Count - count - 2, 6].TextString = tangCaoMax.ToString();
            acTable.Cells[acTable.Rows.Count - count - 2, 7].TextString = (mDXDmax / 100 * tangCaoMax).ToString("F2");
        }

        //method to get center point of polyline
        internal static Point3d GetCenterPoint(Polyline pl)
        {
            Point3d cenPt = new Point3d();
            Point3d pt = pl.GeometricExtents.MaxPoint;
            cenPt = new Point3d((pl.GeometricExtents.MinPoint.X + pl.GeometricExtents.MaxPoint.X) / 2,
                                (pl.GeometricExtents.MinPoint.Y + pl.GeometricExtents.MaxPoint.Y) / 2, 0);
            return cenPt;
        }

        //method to insert a Block
        internal static BlockReference InsertingABlock(Database db, Transaction acTrans, string blockName, Point3d originPt)
        {
            //Open the block table for read
            BlockTable acBlkTbl;
            acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            ObjectId blkRecId = ObjectId.Null;
            if (!acBlkTbl.Has(blockName))
            {
                using (BlockTableRecord acBlkTblRec = new BlockTableRecord())
                {
                    acBlkTblRec.Name = blockName;

                    //Set the insertion point for the block
                    acBlkTblRec.Origin = originPt;

                    blkRecId = acBlkTblRec.Id;
                }
            }
            else
            {
                blkRecId = acBlkTbl[blockName];
            }
            // Insert the block into the current space
            if (blkRecId != ObjectId.Null)
            {
                using (BlockReference acBlkRef = new BlockReference(new Point3d(0, 0, 0), blkRecId))
                {
                    BlockTableRecord acCurSpaceBlkTblRec;
                    acCurSpaceBlkTblRec = acTrans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                    acCurSpaceBlkTblRec.AppendEntity(acBlkRef);
                    acTrans.AddNewlyCreatedDBObject(acBlkRef, true);
                    return acBlkRef;
                }
            }
            return null;

        }

        //method to copy entitie in the current model space
        internal static void CopyEntities(Database db, Transaction acTrans, Entity ent, Point3d pt)
        {
            using (Entity entCopy = ent.Id.GetObject(OpenMode.ForRead) as Entity)
            {
                if (entCopy != null)
                {
                    Entity entCopy1 = entCopy.Clone() as Entity;
                    entCopy1.TransformBy(Matrix3d.Displacement(pt.GetAsVector()));
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = acTrans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                    acBlkTblRec.AppendEntity(entCopy1);
                    acTrans.AddNewlyCreatedDBObject(entCopy1, true);
                }
            }

        }
        //method to check existing Layer
        internal static Boolean IsExistingLayer(Transaction trans, LayerTable ltb, string layerName)
        {
            LayerTableRecord Lr;
            foreach (ObjectId id in ltb)
            {
                Lr = trans.GetObject(id, OpenMode.ForRead) as LayerTableRecord;
                if (Lr.Name == layerName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}