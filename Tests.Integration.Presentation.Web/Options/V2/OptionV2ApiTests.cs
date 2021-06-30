using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.XUnit;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Options.V2
{
    [Collection(nameof(SequentialTestGroup))]
    public class OptionV2ApiTests : WithAutoFixture
    {
        [Theory, MemberData(nameof(GetV2ResourceNames))]
        public async Task Can_Get_AvailableOptions( string apiv2OptionResource)
        {
            //Arrange
            var orgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);
            var pageSize = Math.Max(1, A<int>() % 2); //Minimum is 1;
            var pageNumber = 0; //Always takes the first page;

            //Act
            var optionTypes = (await OptionV2ApiHelper.GetOptionsAsync(apiv2OptionResource, orgUuid, pageSize, pageNumber)).ToList();

            //Assert
            Assert.Equal(pageSize, optionTypes.Count);
        }

        [Theory, MemberData(nameof(GetV1AndV2ResourceNames))]
        public async Task Can_Get_Specific_Option_That_Is_Available(string apiv1OptionResource, string apiv2OptionResource)
        {
            //Arrange
            var orgId = TestEnvironment.DefaultOrganizationId;
            var name = A<string>();
            await EntityOptionHelper.CreateOptionTypeAsync(apiv1OptionResource, name, orgId);
            var organisation = await OrganizationHelper.GetOrganizationAsync(orgId);
            var organisationUuid = organisation.Uuid.GetValueOrDefault();
            var options = await OptionV2ApiHelper.GetOptionsAsync(apiv2OptionResource, organisationUuid, 100, 0); //100 should be more than enough to get all.
            var option = options.First(x => x.Name.Equals(name)); //Get the newly created type.

            //Act
            var result = await OptionV2ApiHelper.GetOptionAsync(apiv2OptionResource, option.Uuid, organisationUuid);

            //Assert
            Assert.Equal(name, result.Name);
            Assert.Equal(option.Uuid, result.Uuid);
            Assert.True(result.IsAvailable);
        }

        [Theory,MemberData(nameof(GetV1AndV2ResourceNames))]
        public async Task Can_Get_Specific_Option_That_Is_Not_Available(string apiv1OptionResource, string apiv2OptionResource)
        {
            //Arrange
            var orgId = TestEnvironment.DefaultOrganizationId;
            var newName = A<string>();
            var createdType = await EntityOptionHelper.CreateOptionTypeAsync(apiv1OptionResource, newName, orgId);
            var organisation = await OrganizationHelper.GetOrganizationAsync(orgId);
            var organisationUuid = organisation.Uuid.GetValueOrDefault();
            var options = await OptionV2ApiHelper.GetOptionsAsync(apiv2OptionResource, organisationUuid, 100, 0); //100 should be more than enough to get all.
            var option = options.First(x => x.Name.Equals(newName)); //Get the newly created type.

            //Disable the option
            await EntityOptionHelper.SendChangeOptionIsObligatoryAsync(apiv1OptionResource, createdType.Id, false);

            //Act
            var result = await OptionV2ApiHelper.GetOptionAsync(apiv2OptionResource, option.Uuid, organisationUuid);

            //Assert
            Assert.Equal(option.Name, result.Name);
            Assert.Equal(option.Uuid, result.Uuid);
            Assert.False(result.IsAvailable);
        }

        public static IEnumerable<object[]> GetV2ResourceNames()
        {
            foreach (var v1AndV2ResourceName in GetV1AndV2ResourceNames())
            {
                yield return new[] {v1AndV2ResourceName[1]};
            }
        }

        public static IEnumerable<object[]> GetV1AndV2ResourceNames()
        {
            yield return new[] { EntityOptionHelper.ResourceNames.BusinessType, OptionV2ApiHelper.ResourceName.BusinessType };
            yield return new[] { EntityOptionHelper.ResourceNames.ItSystemCategories, OptionV2ApiHelper.ResourceName.ItSystemUsageDataClassification };
            yield return new[] { EntityOptionHelper.ResourceNames.FrequencyTypes, OptionV2ApiHelper.ResourceName.ItSystemUsageRelationFrequencies };
            yield return new[] { EntityOptionHelper.ResourceNames.ArchiveTypes, OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTypes };
        }
    }
}
