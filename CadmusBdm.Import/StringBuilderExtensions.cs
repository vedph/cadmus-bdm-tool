using System.Collections.Generic;
using System.Text;

namespace CadmusBdm.Import;

public static class StringBuilderExtensions
{
    private static void FlattenAndTrim(StringBuilder sb)
    {
        // head
        int i = 0;
        while (i < sb.Length && char.IsWhiteSpace(sb[i])) i++;
        if (i > 0)
        {
            sb.Remove(0, i);
            i = 0;
        }

        // body
        while (i < sb.Length)
        {
            if (char.IsWhiteSpace(sb[i]))
            {
                sb[i++] = ' ';
                int start = i;
                while (i < sb.Length && char.IsWhiteSpace(sb[i])) i++;
                if (i > start) sb.Remove(start, i - start);
            }
            else i++;
        }

        // tail
        if (sb.Length > 0 && sb[^1] == ' ')
            sb.Remove(sb.Length - 1, 1);
    }

    /// <summary>
    /// Normalizes whitespace, by flattening all the whitespace(s) sequence
    /// into a single space, trimming at both edges. Eventually it also
    /// removes whitespace at left of any character in <paramref name="noLeftWS"/>,
    /// and at right of any character in <paramref name="noRightWS"/>.
    /// </summary>
    /// <param name="sb">The sb.</param>
    /// <param name="noLeftWS">The no left ws.</param>
    /// <param name="noRightWS">The no right ws.</param>
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
                    sb.Remove(i + 1, 1);
                i--;
            }
        }
    }
}
