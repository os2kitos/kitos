using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Toolkit.Extensions
{
    public static class EnumRange
    {
        public static IEnumerable<T> All<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static IEnumerable<T> AllExcept<T>(params T[] excluded) where T : Enum
        {
            return All<T>().Except(excluded);
        }
    }
}
