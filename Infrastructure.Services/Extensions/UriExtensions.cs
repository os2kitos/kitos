using System;

namespace Infrastructure.Services.Extensions
{
    public static class UriExtensions
    {
        public static bool IsHttpUri(this Uri uri)
        {
            var scheme = uri.Scheme;
            return MatchScheme("http", scheme) || MatchScheme("https", scheme);
        }

        private static bool MatchScheme(string expected, string actual)
        {
            return string.Compare(expected, actual, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
