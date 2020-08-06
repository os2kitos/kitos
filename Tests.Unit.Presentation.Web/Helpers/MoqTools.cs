using System;
using Moq;

namespace Tests.Unit.Presentation.Web.Helpers
{
    public static class MoqTools
    {
        public static object MockedObjectFrom(Type type)
        {
            var mockType = typeof(Mock<>).MakeGenericType(type);
            var baseCtor = mockType.GetConstructor(new Type[0]);
            if (baseCtor == null)
            {
                throw new InvalidOperationException("Unable to determine ctor");
            }

            dynamic mock = baseCtor.Invoke(new object[0]);

            return mock.Object;
        }
    }
}
