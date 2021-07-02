using System;
using Presentation.Web.Infrastructure.Attributes;
using Xunit;

namespace Tests.Unit.Presentation.Web.Infrastructure
{
    public class NonEmptyGuidAttributeTest
    {
        private readonly NonEmptyGuidAttribute _sut;

        public NonEmptyGuidAttributeTest()
        {
            _sut = new NonEmptyGuidAttribute();
        }

        [Fact]
        public void IsValid_Returns_False_For_Empty_Guid()
        {
            var input = Guid.Empty;

            var valid = _sut.IsValid(input);

            Assert.False(valid);
        }

        [Fact]
        public void IsValid_Returns_False_For_Empty_NullableGuid()
        {
            Guid? input = Guid.Empty;

            var valid = _sut.IsValid(input);

            Assert.False(valid);
        }

        [Fact]
        public void IsValid_Returns_True_For_NonEmpty_Guid()
        {
            var input = Guid.NewGuid();

            var valid = _sut.IsValid(input);

            Assert.True(valid);
        }

        [Fact]
        public void IsValid_Returns_True_For_Null_NullableGuid()
        {
            var input = default(Guid?);

            var valid = _sut.IsValid(input);

            Assert.True(valid);
        }
    }
}
