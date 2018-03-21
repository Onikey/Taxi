using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Taxi.DataAccess.Censore
{
    public static class CensoreManager
    {
        public static string FilterText(this string input)
        {
            var sb = new StringBuilder();

            var words = CensoruWords();
            foreach (var item in words)
            {
                if (words.First() != item)
                    sb.Append("|");

                sb.Append($"({item})");
            }

            var regex = new Regex(sb.ToString(), RegexOptions.IgnoreCase);

            return regex.Replace(input, "@@@@");
        }

        //We can use any dictionary or xml/json file instead of this
        private static List<string> CensoruWords()
            => new List<string>
            {
                "rediska",
                "slowpoke"
            };
    }
}
