﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DokanNet
{
    public class HelperMethods
    {
        /// <summary>
        /// Matches zero or more characters until encountering and matching the final . in the name.
        /// </summary>
        const Char DOS_STAR = '<';

        /// <summary>
        /// Matches any single character or, upon encountering a period or end
        /// of name string, advances the expression to the end of the set of
        /// contiguous DOS_QMs.
        /// </summary>
        const Char DOS_QM = '>';

        /// <summary>
        /// Matches either a period or zero characters beyond the name string.
        /// </summary>
        const Char DOS_DOT = '"';

        /// <summary>
        /// Matches zero or more characters.
        /// </summary>
        const Char ASTERISK = '*';

        /// <summary>
        /// Matches a single character.
        /// </summary>
        const Char QUESTION_MARK = '?';

        private static Char[] CharsThatMatchEmptyStringsAtEnd = new Char[] { DOS_DOT, DOS_STAR, ASTERISK };

        /// <summary>
        /// Check whether <paramref name="Name">Name</paramref> matches <paramref name="Expression">Expression</paramref>.
        /// </summary>
        /// <remarks>
        /// This method is mainly used in <see cref="IDokanOperations.FindFilesWithPattern"/> to filter a list of possible files.
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/ff546850(v=VS.85).aspx"/> 
        /// </remarks>
        /// <param name="Expression">The matching pattern. Can contain: ?, *, &lt;, &quot;, &gt;.</param>
        /// <param name="Name">The string that will be tested.</param>
        /// <param name="IgnoreCase">When set to true a case insensitive match will be performed.</param>
        /// <returns>Returns true if Expression match Name, false otherwise.</returns>
        /// <example>"F0_&lt;&quot;*" match "f0_001.txt"</example>
        public static Boolean DokanIsNameInExpression(String Expression, String Name, Boolean IgnoreCase)
        {
            var ei = 0;
            var ni = 0;

            while (ei < Expression.Length && ni < Name.Length)
            {
                if (Expression[ei] == ASTERISK)
                {
                    ei++;
                    if (ei > Expression.Length)
                        return true;

                    while (ni < Name.Length)
                    {
                        if (DokanIsNameInExpression(Expression.Substring(ei), Name.Substring(ni), IgnoreCase))
                            return true;
                        ni++;
                    }

                }
                else if (Expression[ei] == DOS_STAR)
                {
                    var lastDotIndex = Name.LastIndexOf('.');
                    ei++;

                    var endReached = false;
                    while (!endReached)
                    {
                        endReached = (ni >= Name.Length || lastDotIndex > -1 && ni > lastDotIndex);

                        if (!endReached)
                        {
                            if (DokanIsNameInExpression(Expression.Substring(ei), Name.Substring(ni), IgnoreCase))
                                return true;
                            ni++;
                        }
                    }
                }
                else if (Expression[ei] == DOS_QM)
                {
                    ei++;
                    if (Name[ni] != '.')
                    {
                        ni++;
                    }
                    else
                    {
                        var p = ni + 1;
                        while (p < Name.Length)
                        {
                            if (Name[p] == '.')
                                break;
                            p++;
                        }

                        if (p < Name.Length && Name[p] == '.')
                            ni++;
                    }
                }
                else if (Expression[ei] == DOS_DOT)
                {
                    if (ei < Expression.Length)
                    {
                        if (Name[ni] != '.')
                            return false;
                        else
                            ni++;
                    }
                    else
                    {
                        if (Name[ni] == '.')
                            ni++;
                    }
                    ei++;
                }
                else
                {
                    if (Expression[ei] == QUESTION_MARK)
                    {
                        ei++;
                        ni++;
                    }
                    else if (IgnoreCase && char.ToUpperInvariant(Expression[ei]) == char.ToUpperInvariant(Name[ni]))
                    {
                        ei++;
                        ni++;
                    }
                    else if (!IgnoreCase && Expression[ei] == Name[ni])
                    {
                        ei++;
                        ni++;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            var nextExpressionChars = Expression.Substring(ei);
            var AreNextExpressionCharsAllNullMatchers = Expression.Any() && !String.IsNullOrEmpty(nextExpressionChars) && nextExpressionChars.All(x => CharsThatMatchEmptyStringsAtEnd.Contains(x));
            var IsNameCurrentCharTheLast = ni == Name.Length;
            if (ei == Expression.Length && IsNameCurrentCharTheLast || IsNameCurrentCharTheLast && AreNextExpressionCharsAllNullMatchers)
                return true;

            return false;
        }
    }
}