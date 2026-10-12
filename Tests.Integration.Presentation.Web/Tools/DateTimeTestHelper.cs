using System;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    internal static class DateTimeTestHelper
    {
        public static void AssertEqual(DateTime? expected, DateTime? actual)
        {
            Assert.Equal(Normalize(expected), Normalize(actual));
        }

        public static void AssertEqual(DateTime expected, DateTime actual)
        {
            Assert.Equal(Normalize(expected), Normalize(actual));
        }

        public static DateTime Normalize(DateTime value)
        {
            var utcValue = value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };

            var truncatedTicks = utcValue.Ticks - (utcValue.Ticks % 10);
            return new DateTime(truncatedTicks, DateTimeKind.Utc);
        }

        public static DateTime? Normalize(DateTime? value)
        {
            return value.HasValue ? Normalize(value.Value) : null;
        }
    }
}
