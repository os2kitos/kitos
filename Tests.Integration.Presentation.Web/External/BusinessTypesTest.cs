using Core.DomainModel.Organization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.External
{
    public class BusinessTypesTest : WithAutoFixture
    {
        [Fact]
        public async Task Can_Get_AvailableBusinessTypes()
        {
            //Arrange
            var orgUuid = TestEnvironment.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);
            var pageSize = Math.Max(1, A<int>() % 2); //Minimum is 1;
            var pageNumber = 0; //Always takes the first page;

            //Act
            var businessTypes = (await BusinessTypeV2Helper.GetBusinessTypesAsync(orgUuid, pageSize, pageNumber)).ToList();

            //Assert
            Assert.Equal(pageSize, businessTypes.Count);
        }

        [Fact]
        public async Task Can_Get_SpecificBusinessType_That_Is_Available()
        {
            //Arrange
            var orgId = TestEnvironment.DefaultOrganizationId;
            var businessTypeName = A<string>();
            await EntityOptionHelper.CreateBusinessTypeAsync(businessTypeName, orgId);
            var organisation = await OrganizationHelper.GetOrganizationAsync(orgId);
            var organisationUuid = organisation.Uuid.GetValueOrDefault();
            var businessTypes = await BusinessTypeV2Helper.GetBusinessTypesAsync(organisationUuid, 100, 0); //100 should be more than enough to get all.
            var businessType = businessTypes.First(x => x.Name.Equals(businessTypeName)); //Get the newly created businessType.

            //Act
            var businessTypeResult = await BusinessTypeV2Helper.GetBusinessTypeAsync(businessType.Uuid, organisationUuid);

            //Assert
            Assert.Equal(businessTypeName, businessTypeResult.Name);
            Assert.Equal(businessType.Uuid, businessTypeResult.Uuid);
            Assert.True(businessTypeResult.IsAvailable);
        }

        [Fact]
        public async Task Can_Get_SpecificBusinessType_That_Is_Not_Available()
        {
            //Arrange
            var orgId = TestEnvironment.DefaultOrganizationId;
            var businessTypeName = A<string>();
            var createdBusinessType = await EntityOptionHelper.CreateBusinessTypeAsync(businessTypeName, orgId);
            var organisation = await OrganizationHelper.GetOrganizationAsync(orgId);
            var organisationUuid = organisation.Uuid.GetValueOrDefault();
            var businessTypes = await BusinessTypeV2Helper.GetBusinessTypesAsync(organisationUuid, 100, 0); //100 should be more than enough to get all.
            var businessType = businessTypes.First(x => x.Name.Equals(businessTypeName)); //Get the newly created businessType.

            //Disable businessType
            await EntityOptionHelper.SendChangeBusinessTypeIsObligatoryAsync(createdBusinessType.Id, false);

            //Act
            var businessTypeResult = await BusinessTypeV2Helper.GetBusinessTypeAsync(businessType.Uuid, organisationUuid);

            //Assert
            Assert.Equal(businessType.Name, businessTypeResult.Name);
            Assert.Equal(businessType.Uuid, businessTypeResult.Uuid);
            Assert.False(businessTypeResult.IsAvailable);
        }
    }
}
