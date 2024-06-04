using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

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

        public static void SortStringList(List<string> stringList)
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

        public static List<Entity> SortBlocksByAttributeList(List<Entity> blocks, Transaction acTrans)
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

        // method to add two numbers  


    }
}
