using Core.DomainServices.SSO;
using Xunit;

namespace Tests.Unit.Core.Model.SSO
{
    public class StsPersonDataTest
    {
        [Theory]
        [InlineData("Ib Ibbermand", "Ib", "Ibbermand")]
        [InlineData("Niels Erik Nordberg", "Niels", "Erik Nordberg")]
        [InlineData("JustMe", "JustMe", "")]
        [InlineData("Tra la la", "Tra", "la la")]
        [InlineData("", "", "")]
        [InlineData("StrangeChars! %%&-?", "StrangeChars!", "%%&-?")]
        public void Can_StsPersonData_SeparateFullNameIntoFirstAndLastNames(string fullname, string expectedFirstName, string expectedLastName)
        {
            var sut = new StsPersonData(fullname);
            Assert.Equal(expectedFirstName, sut.FirstName);
            Assert.Equal(expectedLastName, sut.LastName);
        }
    }
}
