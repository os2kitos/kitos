using System;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations
{
    public class OrganizationUnitTests : WithAutoFixture
    {
        [Fact]
        public async Task Can_Update_Organization_Unit()
        {
            //Arrange
            var organizationDto = await CreateOrganizationAsync();
            var root = await OrganizationUnitHelper.GetOrganizationUnitsAsync(organizationDto.Id);
            var newUnit1 = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organizationDto.Id, A<string>(), root.Id);
            var newUnit2 = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organizationDto.Id, A<string>(), root.Id);

            var newName = A<string>();
            var newParentId = newUnit2.Id;
            var newLocalID = A<string>();
            var newEan = new Random().Next(1, 9999);

            //Act
            var updatedUnit = await OrganizationUnitHelper.PatchOrganizationUnitsAsync(organizationDto.Id, newUnit1.Id, newParentId, newName, newEan, newLocalID);

            //Assert
            Assert.Equal(newName, updatedUnit.Name);
            Assert.Equal(newParentId, updatedUnit.ParentId);
            Assert.Equal(newLocalID, updatedUnit.LocalId);
            Assert.Equal(newEan, updatedUnit.Ean);
        }
        private async Task<OrganizationDTO> CreateOrganizationAsync()
        {
            var organizationName = $"{nameof(OrganizationUnitTests)}_{A<Guid>():N}";
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, organizationName, "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            return organization;
        }

        private string CreateEmail()
        {
            return $"{nameof(OrganizationUnitTests)}{A<string>()}@test.dk";
        }
    }
}
