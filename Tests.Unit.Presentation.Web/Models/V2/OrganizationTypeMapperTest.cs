using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class OrganizationTypeMapperTest
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]

        [InlineData(3)]

        [InlineData(4)]
        public void Can_Map_Organization_Type(int expected)
        {
            var expectedOrgType = new OrganizationType(){Id = expected};
            var sut = new OrganizationTypeMapper();

            var result = sut.MapOrganizationType(expectedOrgType);

            Assert.Equal(expected, (int)result);

        }
    }
}
