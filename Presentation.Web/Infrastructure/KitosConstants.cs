using System;
using System.Globalization;

namespace Presentation.Web.Infrastructure
{
    public static class KitosConstants
    {
        public static readonly int MaxHangfireRetries = 3;
        public static readonly StringComparer DanishStringComparer = StringComparer.Create(new CultureInfo("da-DK"), true);
    }
}