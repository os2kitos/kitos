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
        public async Task Can_Get_AvailableOptions(string apiv2OptionResource)
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
            var organizationDto = await OrganizationHelper.GetOrganizationAsync(orgId);
            var organizationUuid = organizationDto.Uuid.GetValueOrDefault();
            var options = await OptionV2ApiHelper.GetOptionsAsync(apiv2OptionResource, organizationUuid, 100, 0); //100 should be more than enough to get all.
            var option = options.First(x => x.Name.Equals(name)); //Get the newly created type.

            //Act
            var result = await OptionV2ApiHelper.GetOptionAsync(apiv2OptionResource, option.Uuid, organizationUuid);

            //Assert
            Assert.Equal(name, result.Name);
            Assert.Equal(option.Uuid, result.Uuid);
            Assert.True(result.IsAvailable);
        }

        [Theory, MemberData(nameof(GetV1AndV2ResourceNames))]
        public async Task Can_Get_Specific_Option_That_Is_Not_Available(string apiv1OptionResource, string apiv2OptionResource)
        {
            //Arrange
            var orgId = TestEnvironment.DefaultOrganizationId;
            var newName = A<string>();
            var createdType = await EntityOptionHelper.CreateOptionTypeAsync(apiv1OptionResource, newName, orgId);
            var organizationDto = await OrganizationHelper.GetOrganizationAsync(orgId);
            var organizationUuid = organizationDto.Uuid.GetValueOrDefault();
            var options = await OptionV2ApiHelper.GetOptionsAsync(apiv2OptionResource, organizationUuid, 100, 0); //100 should be more than enough to get all.
            var option = options.First(x => x.Name.Equals(newName)); //Get the newly created type.

            //Disable the option
            await EntityOptionHelper.SendChangeOptionIsObligatoryAsync(apiv1OptionResource, createdType.Id, false);

            //Act
            var result = await OptionV2ApiHelper.GetOptionAsync(apiv2OptionResource, option.Uuid, organizationUuid);

            //Assert
            Assert.Equal(option.Name, result.Name);
            Assert.Equal(option.Uuid, result.Uuid);
            Assert.False(result.IsAvailable);
        }

        [Theory, MemberData(nameof(GetRoleResources))]
        public async Task Can_Get_WriteAccess_Status_For_Role_Options(string apiv1OptionResource, string apiv2OptionResource)
        {
            //Arrange
            var writeAccess = A<bool>();
            var orgId = TestEnvironment.DefaultOrganizationId;
            var name = A<string>();
            await EntityOptionHelper.CreateRoleOptionTypeAsync(apiv1OptionResource, name, orgId, writeAccess);
            var organizationDto = await OrganizationHelper.GetOrganizationAsync(orgId);
            var organizationUuid = organizationDto.Uuid.GetValueOrDefault();
            var options = await OptionV2ApiHelper.GetOptionsAsync(apiv2OptionResource, organizationUuid, 100, 0); //100 should be more than enough to get all.
            var option = options.First(x => x.Name.Equals(name)); //Get the newly created type.

            //Act
            var result = await OptionV2ApiHelper.GetRoleOptionAsync(apiv2OptionResource, option.Uuid, organizationUuid);

            //Assert
            Assert.Equal(name, result.Name);
            Assert.Equal(option.Uuid, result.Uuid);
            Assert.True(result.IsAvailable);
            Assert.Equal(writeAccess, result.WriteAccess);
        }

        public static IEnumerable<object[]> GetV2ResourceNames()
        {
            foreach (var v1AndV2ResourceName in GetV1AndV2ResourceNames())
            {
                yield return new[] { v1AndV2ResourceName[1] };
            }
        }

        public static IEnumerable<object[]> GetV1AndV2ResourceNames()
        {
            return GetRegularResources().Concat(GetRoleResources());
        }

        public static IEnumerable<object[]> GetRoleResources()
        {
            yield return new[] { EntityOptionHelper.ResourceNames.SystemRoles, OptionV2ApiHelper.ResourceName.ItSystemUsageRoles };
        }

        public static IEnumerable<object[]> GetRegularResources()
        {
            yield return new[] { EntityOptionHelper.ResourceNames.BusinessType, OptionV2ApiHelper.ResourceName.BusinessType };
            yield return new[] { EntityOptionHelper.ResourceNames.ItSystemCategories, OptionV2ApiHelper.ResourceName.ItSystemUsageDataClassification };
            yield return new[] { EntityOptionHelper.ResourceNames.FrequencyTypes, OptionV2ApiHelper.ResourceName.ItSystemUsageRelationFrequencies };
            yield return new[] { EntityOptionHelper.ResourceNames.ArchiveTypes, OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTypes };
            yield return new[] { EntityOptionHelper.ResourceNames.ArchiveLocations, OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveLocations };
            yield return new[] { EntityOptionHelper.ResourceNames.ArchiveTestLocations, OptionV2ApiHelper.ResourceName.ItSystemUsageArchiveTestLocations };
            yield return new[] { EntityOptionHelper.ResourceNames.SensitivePersonalDataTypes, OptionV2ApiHelper.ResourceName.ItSystemSensitivePersonalDataTypes };
            yield return new[] { EntityOptionHelper.ResourceNames.RegisterTypes, OptionV2ApiHelper.ResourceName.ItSystemUsageRegisterTypes };
            yield return new[] { EntityOptionHelper.ResourceNames.ContractTypes, OptionV2ApiHelper.ResourceName.ItContractContractTypes };
            yield return new[] { EntityOptionHelper.ResourceNames.DataProcessingDataResponsibleOptions, OptionV2ApiHelper.ResourceName.DataProcessingRegistrationDataResponsible };
            yield return new[] { EntityOptionHelper.ResourceNames.DataProcessingBasisForTransferOptions, OptionV2ApiHelper.ResourceName.DataProcessingRegistrationBasisForTransfer };
        }
    }
}
