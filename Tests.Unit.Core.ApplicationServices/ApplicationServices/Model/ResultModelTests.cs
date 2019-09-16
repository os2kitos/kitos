using System;
using Core.ApplicationServices.Model.Result;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Model
{
    public class ResultModelTests
    {

        [Theory]
        [InlineData("string test")]
        [InlineData(12)]
        [InlineData('C')]
        public void Result_Ok_Makes_Ok_With_Value<T>(T input)
        {
            var result = Result<OperationResult, T>.Ok(input);
            Assert.Equal(OperationResult.Ok, result.Status);
            Assert.Equal(input, result.Value);
        }

        [Theory]
        [InlineData(OperationResult.Forbidden, "string test")]
        [InlineData(OperationResult.NotFound, "string test")]
        [InlineData(OperationResult.UnknownError, "string test")]
        public void Result_Fail_Makes_Status_With_Default_Value<T>(OperationResult code, T input)
        {
            var result = Result<OperationResult, T>.Fail(code);
            Assert.Equal(code, result.Status);
            Assert.Null(result.Value);
        }

        [Fact]
        public void Result_Fail_With_Ok_Status_Throws_Argument_Exception()
        {
            Assert.Throws<ArgumentException>(() => Result<OperationResult, string>.Fail(OperationResult.Ok));
        }

    }
}
