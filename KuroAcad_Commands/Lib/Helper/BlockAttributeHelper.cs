using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

namespace KuroAcad.Helper
{
    /// <summary>
    /// Helper methods for reading block attributes and writing block data to AutoCAD tables.
    /// </summary>
    internal static class BlockAttributeHelper
    {
        private const string TagGroup = "A";
        private const string TagName = "TEN";
        private const string TagArea = "DT";
        private const string TagDensity = "MDXD";
        private const string TagFloor = "TC";

        /// <summary>
        /// Sorts block entities by the value of attribute tag "A".
        /// </summary>
        /// <param name="blocks">List of block entities.</param>
        /// <param name="transaction">Active transaction.</param>
        /// <returns>Sorted list of entities.</returns>
        internal static List<Entity> SortBlocksByAttributeList(List<Entity> blocks, Transaction transaction)
        {
            return blocks
                .OfType<BlockReference>()
                .Select(block => new
                {
                    Entity = (Entity)block,
                    SortValue = GetAttributeValue(block, transaction, TagGroup) ?? string.Empty
                })
                .OrderBy(x => x.SortValue, StringComparer.CurrentCulture)
                .Select(x => x.Entity)
                .ToList();
        }

        /// <summary>
        /// Gets blocks whose "TEN" attribute starts with the specified character.
        /// </summary>
        /// <param name="blocks">List of block entities.</param>
        /// <param name="transaction">Active transaction.</param>
        /// <param name="key">First character to filter.</param>
        /// <returns>Filtered list of entities.</returns>
        internal static List<Entity> GetBlockAttributes(List<Entity> blocks, Transaction transaction, char key)
        {
            return blocks
                .OfType<BlockReference>()
                .Where(block =>
                {
                    string name = GetAttributeValue(block, transaction, TagName);
                    return !string.IsNullOrWhiteSpace(name) && name[0] == key;
                })
                .Cast<Entity>()
                .ToList();
        }

        /// <summary>
        /// Gets distinct first characters from the "TEN" attribute of the given blocks.
        /// </summary>
        /// <param name="blocks">List of block entities.</param>
        /// <param name="transaction">Active transaction.</param>
        /// <returns>Distinct list of first characters.</returns>
        internal static List<char> GetListFirstChar(List<Entity> blocks, Transaction transaction)
        {
            return blocks
                .OfType<BlockReference>()
                .Select(block => GetAttributeValue(block, transaction, TagName))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name[0])
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Adds block data into an AutoCAD table.
        /// </summary>
        /// <param name="table">Target table.</param>
        /// <param name="blocks">List of block entities.</param>
        /// <param name="transaction">Active transaction.</param>
        /// <param name="groupKey">Group label to display in summary row.</param>
        internal static void AddDataToTable(Table table, List<Entity> blocks, Transaction transaction, char groupKey)
        {
            if (table == null || blocks == null || blocks.Count == 0)
            {
                return;
            }

            int count = blocks.Count;
            table.InsertRows(table.Rows.Count, 1, count + 1);

            int summaryRowIndex = table.Rows.Count - count - 2;
            int startDataRowIndex = summaryRowIndex + 1;

            double totalArea = 0;

            string lastName = string.Empty;
            double lastDensity = 0;
            int lastFloor = 0;

            for (int i = 0; i < count; i++)
            {
                if (blocks[i] is not BlockReference blockReference)
                {
                    continue;
                }

                string blockName = GetAttributeValue(blockReference, transaction, TagName) ?? string.Empty;
                double blockArea = ParseDouble(GetAttributeValue(blockReference, transaction, TagArea)) / 10000.0;
                double maxDensity = ParseDouble(GetAttributeValue(blockReference, transaction, TagDensity)) / 10.0;
                int maxFloor = ParseInt(GetAttributeValue(blockReference, transaction, TagFloor));

                double floorAreaRatio = maxDensity / 100.0 * maxFloor;
                totalArea += blockArea;

                int rowIndex = startDataRowIndex + i;

                table.Cells[rowIndex, 0].TextString = (i + 1).ToString();
                table.Cells[rowIndex, 1].TextString = blockName;
                table.Cells[rowIndex, 2].TextString = "1";
                table.Cells[rowIndex, 3].TextString = blockArea.ToString("F2", CultureInfo.InvariantCulture);
                table.Cells[rowIndex, 5].TextString = maxDensity.ToString(CultureInfo.InvariantCulture);
                table.Cells[rowIndex, 6].TextString = maxFloor.ToString();
                table.Cells[rowIndex, 7].TextString = floorAreaRatio.ToString("F2", CultureInfo.InvariantCulture);

                lastName = blockName;
                lastDensity = maxDensity;
                lastFloor = maxFloor;
            }

            table.Cells[summaryRowIndex, 1].TextString = groupKey.ToString();
            table.Cells[summaryRowIndex, 2].TextString = count.ToString();
            table.Cells[summaryRowIndex, 4].TextString = totalArea.ToString("F2", CultureInfo.InvariantCulture);
            table.Cells[summaryRowIndex, 5].TextString = lastDensity.ToString(CultureInfo.InvariantCulture);
            table.Cells[summaryRowIndex, 6].TextString = lastFloor.ToString();
            table.Cells[summaryRowIndex, 7].TextString = (lastDensity / 100.0 * lastFloor).ToString("F2", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets an attribute value from a block reference by tag.
        /// </summary>
        private static string GetAttributeValue(BlockReference blockReference, Transaction transaction, string tag)
        {
            foreach (ObjectId attributeId in blockReference.AttributeCollection)
            {
                var attribute = transaction.GetObject(attributeId, OpenMode.ForRead) as AttributeReference;
                if (attribute != null && string.Equals(attribute.Tag, tag, StringComparison.OrdinalIgnoreCase))
                {
                    return attribute.TextString;
                }
            }

            return null;
        }

        /// <summary>
        /// Parses string to double safely.
        /// </summary>
        private static double ParseDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result)
                ? result
                : 0;
        }

        /// <summary>
        /// Parses string to int safely.
        /// </summary>
        private static int ParseInt(string value)
        {
            return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int result)
                ? result
                : 0;
        }
    }
}
