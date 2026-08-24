using System.Text;

namespace KuroAcad.Helper
{
    internal static class RomanNumeralHelper
    {
        internal static string ConvertToRoman(int number)
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
    }
}