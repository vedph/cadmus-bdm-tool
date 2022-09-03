using System.Collections.Generic;
using System.Text;

namespace CadmusBdm.Import
{
    public static class StringBuilderExtensions
    {
        private static void FlattenAndTrim(StringBuilder sb)
        {
            // head
            int i = 0;
            while (i < sb.Length && char.IsWhiteSpace(sb[i])) i++;
            if (i > 0) sb.Remove(0, i);

            // body
            while (i < sb.Length)
            {
                if (char.IsWhiteSpace(sb[i]))
                {
                    sb[i] = ' ';
                    i++;
                    while (i < sb.Length && char.IsWhiteSpace(sb[i])) i++;
                }
                else i++;
            }

            // tail
            if (sb.Length > 0 && sb[sb.Length - 1] == ' ')
                sb.Remove(sb.Length - 1, 1);
        }

        public static void NormalizeWS(this StringBuilder sb,
            IList<char>? noLeftWS = null,
            IList<char>? noRightWS = null)
        {
            FlattenAndTrim(sb);

            if (noLeftWS?.Count > 0)
            {
                int i = sb.Length - 1;
                while (i > 0)
                {
                    if (noLeftWS.Contains(sb[i]))
                    {
                        if (sb[--i] == ' ') sb.Remove(i, 1);
                    }
                    else i--;
                }
            }

            if (noRightWS?.Count > 0)
            {
                int i = sb.Length - 2;
                while (i > -1)
                {
                    if (noRightWS.Contains(sb[i]) && sb[i + 1] == ' ')
                        sb.Remove(i, 1);
                    i--;
                }
            }
        }
    }
}
