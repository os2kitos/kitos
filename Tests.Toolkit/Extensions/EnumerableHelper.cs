using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Moq;

namespace Tests.Toolkit.Extensions
{
    public static class EnumerableHelper
    {
        public static T RandomItem<T>(this IEnumerable<T> src)
        {
            var fixture = new Fixture();
            return src.OrderBy(_ => fixture.Create<int>()).First();
        }

        public static IEnumerable<T> RandomItems<T>(this IEnumerable<T> src, int howMany)
        {
            var fixture = new Fixture();
            return src.OrderBy(_ => fixture.Create<int>()).Take(howMany).ToList();
        }

        public static IEnumerable<T> AsParameterMatch<T>(this IEnumerable<T> src)
        {
            return It.Is<IEnumerable<T>>(lst => lst.SequenceEqual(src));
        }
    }
}
