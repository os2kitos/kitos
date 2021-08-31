using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class EnumerableHelper
    {
        public static T RandomItem<T>(this IEnumerable<T> src)
        {
            var fixture = new Fixture();
            return src.OrderBy(_ => fixture.Create<int>()).First();
        }
    }
}
