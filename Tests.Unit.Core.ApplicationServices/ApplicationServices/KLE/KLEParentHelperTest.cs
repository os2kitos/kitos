using Core.DomainServices.Repositories.KLE;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.KLE
{
    public class KLEParentHelperTest
    {
        [Theory]
        [InlineData("12.13.14", "12.13", true)]
        [InlineData("121212.131313.14", "121212.131313", true)]
        [InlineData("12", "", false)]
        private void DeduceParentId_Given_KLE_Number_Returns_ValidParentId(string kleNumber, string expectedParentTaskKey, bool expectedResult)
        {
            var sut = new KLEParentHelper();
            Assert.Equal(expectedResult, sut.TryDeduceParentTaskKey(kleNumber, out var result));
            Assert.Equal(expectedParentTaskKey, result);
        }
    }
}
