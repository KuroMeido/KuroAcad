namespace KuroAcad.Helper
{
    internal static class StringSortHelper
    {
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

                return a.Length.CompareTo(b.Length);
            });
        }
    }
}