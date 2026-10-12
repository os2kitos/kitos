using System;
using System.Text;

namespace Presentation.Web.Infrastructure.OData
{
    public static class ODataQueryStringNormalizer
    {
        public static string NormalizeNestedQueryOptions(string queryString)
        {
            if (string.IsNullOrWhiteSpace(queryString) || queryString.Contains("&$") == false)
            {
                return queryString;
            }

            var builder = new StringBuilder(queryString.Length);
            var depth = 0;
            var inStringLiteral = false;

            for (var i = 0; i < queryString.Length; i++)
            {
                var current = queryString[i];

                if (current == '\'' )
                {
                    if (inStringLiteral)
                    {
                        // An escaped quote inside a string literal is represented as ''.
                        // If the next char is also a quote we stay inside the string; otherwise we close it.
                        if (i + 1 < queryString.Length && queryString[i + 1] == '\'')
                        {
                            // Emit both quotes and skip the second one.
                            builder.Append('\'');
                            builder.Append('\'');
                            i++;
                            continue;
                        }

                        inStringLiteral = false;
                    }
                    else
                    {
                        inStringLiteral = true;
                    }
                }

                if (!inStringLiteral)
                {
                    switch (current)
                    {
                        case '(':
                            depth++;
                            break;
                        case ')':
                            depth = Math.Max(depth - 1, 0);
                            break;
                    }

                    if (current == '&' && depth > 0 && IsNestedSystemQueryOption(queryString, i + 1))
                    {
                        builder.Append(';');
                        continue;
                    }
                }

                builder.Append(current);
            }

            return builder.ToString();
        }

        private static bool IsNestedSystemQueryOption(string queryString, int index)
        {
            return index < queryString.Length && queryString[index] == '$';
        }
    }
}
