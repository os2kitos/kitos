using System;
using Infrastructure.Services.Types;
using Xunit;

namespace Tests.Unit.Core.Infrastructure
{

    public class TypeHierarchyExtensionsTests
    {
        public abstract class GenericSuper<T>
        {

        }

        public class SimpleConcrete 
        {

        }

        public class SpecificGeneric : GenericSuper<string>
        {

        }


        [Theory]
        [InlineData(typeof(SpecificGeneric), typeof(GenericSuper<>), true)]
        [InlineData(typeof(SimpleConcrete), typeof(GenericSuper<>), false)]
        public void IsImplementationOfGenericType_Returns(Type source, Type candidate, bool expectedResult)
        {
            var result = source.IsImplementationOfGenericType(candidate);

            Assert.Equal(expectedResult, result);
        }
    }
}
