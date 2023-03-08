using System;
using System.Globalization;

namespace Presentation.Web.Infrastructure
{
    public static class KitosConstants
    {
        public static readonly int MaxHangfireRetries = 3;
        public static readonly StringComparer DanishStringComparer = StringComparer.Create(new CultureInfo("da-DK"), true);
        public static class Headers
        {
            public const string SerializeEnumAsInteger = "X-KITOS-SERIALIZE-ENUM-AS-INTEGER";
        }
    }
}