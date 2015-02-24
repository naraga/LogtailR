using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogtailR
{
    public class TextChunkSplitter
    {
        /// <summary>
        /// Split chunk into messages.
        /// </summary>
        /// <param name="s">Input chunk</param>
        /// <param name="bomRx">Begining of Message regular expression</param>
        /// <returns></returns>
        public static IEnumerable<string> Split(string s, string bomRx)
        {
            // no BOM specified - yield as single chunk
            if (string.IsNullOrEmpty(bomRx))
            {
                yield return s;
                yield break;
            }

            var matches = Regex.Matches(s, bomRx, RegexOptions.Multiline).OfType<Match>().ToList();

            // no BOM found - yield as single chunk
            if (matches.Count == 0)
            {
                yield return s;
                yield break;
            }

            // yields preamble (anything before first BOM)
            if (matches[0].Index != 0)
                yield return s.Substring(0, matches[0].Index);


            for (int i = 0; i < matches.Count - 1; i++)
            {
                var mIndex = matches[i].Index;
                var mNextIndex = matches[i + 1].Index;
                yield return s.Substring(mIndex, mNextIndex - mIndex);
            }

            // yields footer (anything after last BOM)
            yield return s.Substring(matches.Last().Index);
        }
    }
}