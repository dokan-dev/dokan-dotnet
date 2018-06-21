using System.Linq;

namespace DokanNet
{
    public static class DokanHelper
    {
        /// <summary>
        /// Matches zero or more characters until encountering and matching the final . in the name.
        /// </summary>
        private const char DOS_STAR = '<';

        /// <summary>
        /// Matches any single character or, upon encountering a period or end
        /// of name string, advances the expression to the end of the set of
        /// contiguous DOS_QMs.
        /// </summary>
        private const char DOS_QM = '>';

        /// <summary>
        /// Matches either a period or zero characters beyond the name string.
        /// </summary>
        private const char DOS_DOT = '"';

        /// <summary>
        /// Matches zero or more characters.
        /// </summary>
        private const char ASTERISK = '*';

        /// <summary>
        /// Matches a single character.
        /// </summary>
        private const char QUESTION_MARK = '?';

        private readonly static char[] CharsThatMatchEmptyStringsAtEnd = { DOS_DOT, DOS_STAR, ASTERISK };

        /// <summary>
        /// Check whether <paramref name="name">Name</paramref> matches <paramref name="expression">Expression</paramref>.
        /// </summary>
        /// <remarks>
        /// This method is mainly used in <see cref="IDokanOperations.FindFilesWithPattern"/> to filter a list of possible files.
        /// For example "F0_&lt;&quot;*" match "f0_001.txt"
        /// \see <a href="http://msdn.microsoft.com/en-us/library/ff546850(v=VS.85).aspx">See FsRtlIsNameInExpression routine (MSDN)</a>
        /// </remarks>
        /// <param name="expression">The matching pattern. Can contain: ?, *, &lt;, &quot;, &gt;.</param>
        /// <param name="name">The string that will be tested.</param>
        /// <param name="ignoreCase">When set to true a case insensitive match will be performed.</param>
        /// <returns>Returns true if Expression match Name, false otherwise.</returns>
        public static bool DokanIsNameInExpression(string expression, string name, bool ignoreCase)
        {
            var ei = 0;
            var ni = 0;

            while (ei < expression.Length && ni < name.Length)
            {
                switch (expression[ei])
                {
                    case ASTERISK:
                        ei++;
                        if (ei > expression.Length)
                            return true;

                        while (ni < name.Length)
                        {
                            if (DokanIsNameInExpression(expression.Substring(ei), name.Substring(ni), ignoreCase))
                                return true;
                            ni++;
                        }

                        break;
                    case DOS_STAR:
                        var lastDotIndex = name.LastIndexOf('.');
                        ei++;

                        var endReached = false;
                        while (!endReached)
                        {
                            endReached = (ni >= name.Length || lastDotIndex > -1 && ni > lastDotIndex);

                            if (!endReached)
                            {
                                if (DokanIsNameInExpression(expression.Substring(ei), name.Substring(ni), ignoreCase))
                                    return true;
                                ni++;
                            }
                        }

                        break;
                    case DOS_QM:
                        ei++;
                        if (name[ni] != '.')
                        {
                            ni++;
                        }
                        else
                        {
                            var p = ni + 1;
                            while (p < name.Length)
                            {
                                if (name[p] == '.')
                                    break;
                                p++;
                            }

                            if (p < name.Length && name[p] == '.')
                                ni++;
                        }

                        break;
                    case DOS_DOT:
                        if (ei < expression.Length)
                        {
                            if (name[ni] != '.')
                                return false;
                            else
                                ni++;
                        }
                        else
                        {
                            if (name[ni] == '.')
                                ni++;
                        }
                        ei++;
                        break;
                    case QUESTION_MARK:
                        ei++;
                        ni++;
                        break;
                    default:
                        if (ignoreCase && char.ToUpperInvariant(expression[ei]) == char.ToUpperInvariant(name[ni]))
                        {
                            ei++;
                            ni++;
                        }
                        else if (!ignoreCase && expression[ei] == name[ni])
                        {
                            ei++;
                            ni++;
                        }
                        else
                        {
                            return false;
                        }

                        break;
                }
            }

            var nextExpressionChars = expression.Substring(ei);
            var areNextExpressionCharsAllNullMatchers = expression.Any() && !string.IsNullOrEmpty(nextExpressionChars) && nextExpressionChars.All(x => CharsThatMatchEmptyStringsAtEnd.Contains(x));
            var isNameCurrentCharTheLast = ni == name.Length;
            if (ei == expression.Length && isNameCurrentCharTheLast || isNameCurrentCharTheLast && areNextExpressionCharsAllNullMatchers)
                return true;

            return false;
        }
    }
}
