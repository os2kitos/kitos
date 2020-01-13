using System;
using Core.DomainServices.Model.Result;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Model
{
    public class TwoTrackResultTest
    {
        [Fact]
        public void Success_Returns_OkResult_With_Value()
        {
            var value = Guid.NewGuid();

            var result = TwoTrackResult<Guid, bool>.Success(value);

            Assert.True(result.Ok);
            Assert.Equal(value, result.Value);
            Assert.Throws<InvalidOperationException>(() => result.Error);
        }

        [Fact]
        public void Success_Returns_ErrorResult_With_ErrorValue()
        {
            var value = Guid.NewGuid();

            var result = TwoTrackResult<bool, Guid>.Failure(value);

            Assert.False(result.Ok);
            Assert.Equal(value, result.Error);
            Assert.Throws<InvalidOperationException>(() => result.Value);
        }
    }
}
