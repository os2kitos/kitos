using System.Collections.Generic;

namespace Core.DomainServices.Extensions
{
    public static class StringEnumerableExtensions
    {
        public static string ToStringWithDelimiter(this IEnumerable<string> enumerable, string delimiter = ", ")
        {
            return string.Join(delimiter, enumerable);
        }
    }
}
