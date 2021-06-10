using Core.DomainModel.ItSystem;
using Presentation.Web.Models.External.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.External
{
    public class BusinessTypesTest :WithAutoFixture
    {
        [Fact]
        public async Task Can_Get_AvailableBusinessTypes()
        {
            //Arrange
            var organisation = await OrganizationHelper.GetOrganizationAsync(TestEnvironment.DefaultOrganizationId);
            var pageSize = A<int>() % 9 + 1; //Minimum is 1;
            var pageNumber = 0; //Always takes the first page;
            var data = DatabaseAccess.MapFromEntitySet<BusinessType, IEnumerable<Guid>>(x => x.AsQueryable().OrderBy(x => x.Name).Select(x => x.Uuid).Take(pageSize).ToList());

            //Act
            var businessTypes = await BusinessTypeV2Helper.GetBusinessTypesAsync(organisation.Uuid.Value, pageSize, pageNumber);

            //Assert
            Assert.Equal(pageSize, businessTypes.Count());
            Assert.Collection(businessTypes, x => data.Contains(x.Uuid));
        }

        [Fact]
        public async Task Can_Get_SpecificBusinessType_That_Is_Available()
        {
            //Arrange
            var orgId = TestEnvironment.DefaultOrganizationId;
            var businessTypeName = A<string>();
            var createdBusinessType = await EntityOptionHelper.SendCreateBusinessTypeAsync(businessTypeName, orgId);
            var organisation = await OrganizationHelper.GetOrganizationAsync(orgId);
            var businessTypes = await BusinessTypeV2Helper.GetBusinessTypesAsync(organisation.Uuid.Value, 100, 0); //100 should be more than enough to get all.
            var businessType = businessTypes.Where(x => x.Name.Equals(businessTypeName)).First(); //Get the newly created businessType.

            //Act
            var businessTypeResult = await BusinessTypeV2Helper.GetBusinessTypeAsync(businessType.Uuid, organisation.Uuid.Value);

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
            var createdBusinessType = await EntityOptionHelper.SendCreateBusinessTypeAsync(businessTypeName, orgId);
            var organisation = await OrganizationHelper.GetOrganizationAsync(orgId);
            var businessTypes = await BusinessTypeV2Helper.GetBusinessTypesAsync(organisation.Uuid.Value, 100, 0); //100 should be more than enough to get all.
            var businessType = businessTypes.Where(x => x.Name.Equals(businessTypeName)).First(); //Get the newly created businessType.

            //Disable businessType
            await EntityOptionHelper.SendChangeBusinessTypeIsObligatoryAsync(createdBusinessType.Id, false); 

            //Act
            var businessTypeResult = await BusinessTypeV2Helper.GetBusinessTypeAsync(businessType.Uuid, organisation.Uuid.Value);

            //Assert
            Assert.Equal(businessType.Name, businessTypeResult.Name);
            Assert.Equal(businessType.Uuid, businessTypeResult.Uuid);
            Assert.False(businessTypeResult.IsAvailable);
        }
    }
}
