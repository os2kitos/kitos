using Core.DomainModel;
using Core.DomainModel.Organization;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.GDPR.V2
{
    public class DataProcessingRegistrationApiV2Test : WithAutoFixture
    {
        [Fact]
        public async Task GET_DPR_Returns_Ok()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var newDPR = await DataProcessingRegistrationHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());

            //Act
            var dto = await DataProcessingRegistrationV2Helper.GetDPRAsync(regularUserToken.Token, newDPR.Uuid);

            //Assert
            Assert.Equal(newDPR.Uuid, dto.Uuid);
            Assert.Equal(newDPR.Name, dto.Name);
        }

        [Fact]
        public async Task GET_DPR_Returns_NotFound()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await DataProcessingRegistrationV2Helper.SendGetDPRAsync(regularUserToken.Token, A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GET_DPR_Returns_BadRequest_For_Empty_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await DataProcessingRegistrationV2Helper.SendGetDPRAsync(regularUserToken.Token, Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GET_DPRs_Returns_Ok()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(regularUserToken.Token, 0, 100);

            //Assert
            Assert.NotEmpty(dprs);
        }

        [Fact]
        public async Task GET_DPRs_Returns_Ok_With_OrganizationFiltering()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var newOrg = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "11223344", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            var newDPR = await DataProcessingRegistrationHelper.CreateAsync(newOrg.Id, CreateName());

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(regularUserToken.Token, 0, 100, organizationUuid: newOrg.Uuid);

            //Assert
            var dpr = Assert.Single(dprs);
            Assert.Equal(newDPR.Uuid, dpr.Uuid);
        }

        [Fact]
        public async Task GET_DPRs_Returns_Ok_With_SystemFiltering()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var newOrg = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "11223344", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            var newSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), newOrg.Id, AccessModifier.Local);
            var newSystemUsage = await ItSystemHelper.TakeIntoUseAsync(newSystem.Id, newOrg.Id);
            var newDPR = await DataProcessingRegistrationHelper.CreateAsync(newOrg.Id, CreateName());
            using var assignResult = await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(newDPR.Id, newSystemUsage.Id);
            Assert.Equal(HttpStatusCode.OK, assignResult.StatusCode);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(regularUserToken.Token, 0, 100, systemUuid: newSystem.Uuid);

            //Assert
            var dpr = Assert.Single(dprs);
            Assert.Equal(newDPR.Uuid, dpr.Uuid);
        }

        [Fact]
        public async Task GET_DPRs_Returns_Ok_With_SystemUsageFiltering()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var newOrg = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "11223344", OrganizationTypeKeys.Virksomhed, AccessModifier.Public);
            var newSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), newOrg.Id, AccessModifier.Local);
            var newSystemUsage = await ItSystemHelper.TakeIntoUseAsync(newSystem.Id, newOrg.Id);
            var newDPR = await DataProcessingRegistrationHelper.CreateAsync(newOrg.Id, CreateName());
            using var assignResult = await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(newDPR.Id, newSystemUsage.Id);
            Assert.Equal(HttpStatusCode.OK, assignResult.StatusCode);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(regularUserToken.Token, 0, 100, systemUsageUuid: newSystemUsage.Uuid);

            //Assert
            var dpr = Assert.Single(dprs);
            Assert.Equal(newDPR.Uuid, dpr.Uuid);
        }

        [Fact]
        public async Task GET_DPRs_Returns_BadRequest_For_Empty_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            var response = await DataProcessingRegistrationV2Helper.SendGetDPRsAsync(regularUserToken.Token, 0, 100, systemUsageUuid: Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_POST_With_Name_Only()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var name = CreateName();
            var request = new CreateDataProcessingRegistrationRequestDTO()
            {
                Name = name,
                OrganizationUuid = organization.Uuid
            };

            //Act
            var dto = await DataProcessingRegistrationV2Helper.PostAsync(token, request);

            //Assert
            Assert.Equal(name, dto.Name);
            Assert.NotEqual(Guid.Empty, dto.Uuid);
            Assert.Equal(organization.Name, dto.OrganizationContext.Name);
            Assert.Equal(organization.Cvr, dto.OrganizationContext.Cvr);
            Assert.Equal(organization.Uuid, dto.OrganizationContext.Uuid);
        }

        [Fact]
        public async Task Cannot_POST_With_Duplicate_Name_In_Same_Organization()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var name = CreateName();
            var request = new CreateDataProcessingRegistrationRequestDTO()
            {
                Name = name,
                OrganizationUuid = organization.Uuid
            };

            //Act
            await DataProcessingRegistrationV2Helper.PostAsync(token, request);
            using var duplicateResponse = await DataProcessingRegistrationV2Helper.SendPostAsync(token, request);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
        }

        [Fact]
        public async Task Cannot_POST_If_Organization_Does_Not_Exist()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var name = CreateName();
            var request = new CreateDataProcessingRegistrationRequestDTO()
            {
                Name = name,
                OrganizationUuid = A<Guid>()
            };

            //Act
            using var failingRequest = await DataProcessingRegistrationV2Helper.SendPostAsync(token, request);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, failingRequest.StatusCode);
        }

        [Fact]
        public async Task Cannot_POST_If_User_Is_Not_Member_Of_Organization()
        {
            //Arrange
            var (token, _, _) = await CreatePrerequisitesAsync();
            var defaultOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

            var name = CreateName();
            var request = new CreateDataProcessingRegistrationRequestDTO()
            {
                Name = name,
                OrganizationUuid = defaultOrgUuid
            };

            //Act
            using var failingRequest = await DataProcessingRegistrationV2Helper.SendPostAsync(token, request);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, failingRequest.StatusCode);
        }

        [Fact]
        public async Task Can_PUT_With_Name_Change()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var name1 = CreateName();
            var name2 = CreateName();
            var request = new CreateDataProcessingRegistrationRequestDTO()
            {
                Name = name1,
                OrganizationUuid = organization.Uuid
            };
            var dto = await DataProcessingRegistrationV2Helper.PostAsync(token, request);

            //Act
            var changedDTO = await DataProcessingRegistrationV2Helper.PutAsync(token, dto.Uuid, new DataProcessingRegistrationWriteRequestDTO() { Name = name2 });

            //Assert
            Assert.Equal(name2, changedDTO.Name);
        }

        [Fact]
        public async Task Cannot_PUT_Duplicated_Name()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var name1 = CreateName();
            var name2 = CreateName();
            var createRequest1 = new CreateDataProcessingRegistrationRequestDTO { Name = name1, OrganizationUuid = organization.Uuid };
            var createRequest2 = new CreateDataProcessingRegistrationRequestDTO { Name = name2, OrganizationUuid = organization.Uuid };
            await DataProcessingRegistrationV2Helper.PostAsync(token, createRequest1);
            var dpr2 = await DataProcessingRegistrationV2Helper.PostAsync(token, createRequest2);

            //Act - try to change name of dpr2 to that of dpr1
            using var response = await DataProcessingRegistrationV2Helper.SendPutAsync(token, dpr2.Uuid, new DataProcessingRegistrationWriteRequestDTO() { Name = name1 });

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Theory]
        [InlineData(true, true, true, true, true)]
        [InlineData(true, true, true, true, false)]
        [InlineData(true, true, true, false, true)]
        [InlineData(true, true, false, true, true)]
        [InlineData(true, false, true, true, true)]
        [InlineData(false, true, true, true, true)]
        public async Task Can_POST_With_GeneralData(bool withDataProcessors, bool withSubDataProcessors, bool withResponsible, bool withBasisForTransfer, bool withInsecureCountries)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dataProcessor1 = withDataProcessors ? await CreateOrganizationAsync(A<OrganizationTypeKeys>()) : default;
            var dataProcessor2 = withDataProcessors ? await CreateOrganizationAsync(A<OrganizationTypeKeys>()) : default;
            var subDataProcessor1 = withSubDataProcessors ? await CreateOrganizationAsync(A<OrganizationTypeKeys>()) : default;
            var subDataProcessor2 = withSubDataProcessors ? await CreateOrganizationAsync(A<OrganizationTypeKeys>()) : default;
            var dataResponsible = withResponsible ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationDataResponsible, organization.Uuid, 10, 0)).OrderBy(x => A<int>()).First() : default;
            var basisForTransfer = withBasisForTransfer ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationBasisForTransfer, organization.Uuid, 10, 0)).OrderBy(x => A<int>()).First() : default;
            var country = withInsecureCountries ? (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationCountry, organization.Uuid, 10, 0)).OrderBy(x => A<int>()).First() : default;
            var input = new DataProcessingRegistrationGeneralDataWriteRequestDTO
            {
                DataResponsibleUuid = dataResponsible?.Uuid,
                DataResponsibleRemark = A<string>(),
                IsAgreementConcluded = A<YesNoIrrelevantChoice>(),
                IsAgreementConcludedRemark = A<string>(),
                AgreementConcludedAt = A<DateTime>(),
                BasisForTransferUuid = basisForTransfer?.Uuid,
                TransferToInsecureThirdCountries = withInsecureCountries ? YesNoUndecidedChoice.Yes : YesNoUndecidedChoice.No,
                InsecureCountriesSubjectToDataTransferUuids = country?.Uuid.WrapAsEnumerable().ToList(),
                HasSubDataProcesors = withSubDataProcessors ? YesNoUndecidedChoice.Yes : YesNoUndecidedChoice.No,
                DataProcessorUuids = withDataProcessors ? new[] { dataProcessor1.Uuid, dataProcessor2.Uuid } : null,
                SubDataProcessorUuids = withSubDataProcessors ? new[] { subDataProcessor1.Uuid, subDataProcessor2.Uuid } : null
            };

            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                General = input
            };

            //Act
            var dto = await DataProcessingRegistrationV2Helper.PostAsync(token, request);

            //Assert
            //TODO: Assert the content
        }

        [Theory]
        [InlineData(YesNoUndecidedChoice.No)]
        [InlineData(YesNoUndecidedChoice.Undecided)]
        [InlineData(null)]
        public async Task Cannot_POST_With_GeneralData_And_InsecureThirdCountries_When_TransferToInsecureCountries_Is_Anyhing_But_Yes(YesNoUndecidedChoice? transferToInsecureThirdCountries)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var country = (await OptionV2ApiHelper.GetOptionsAsync(OptionV2ApiHelper.ResourceName.DataProcessingRegistrationCountry, organization.Uuid, 10, 0)).OrderBy(x => A<int>()).First();
            var input = new DataProcessingRegistrationGeneralDataWriteRequestDTO
            {
                TransferToInsecureThirdCountries = transferToInsecureThirdCountries,
                InsecureCountriesSubjectToDataTransferUuids = new[] { country.Uuid }
            };

            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                General = input
            };

            //Act
            using var response = await DataProcessingRegistrationV2Helper.SendPostAsync(token, request);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(YesNoUndecidedChoice.No)]
        [InlineData(YesNoUndecidedChoice.Undecided)]
        [InlineData(null)]
        public async Task Cannot_POST_With_GeneralData_And_SubDataProcessor_When_HasSubDataProcessors_Set_To_Anything_But_Yes(YesNoUndecidedChoice? hasSubDataProcessors)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var input = new DataProcessingRegistrationGeneralDataWriteRequestDTO
            {
                HasSubDataProcesors = hasSubDataProcessors,
                SubDataProcessorUuids = new[] { organization.Uuid }
            };

            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = CreateName(),
                OrganizationUuid = organization.Uuid,
                General = input
            };

            //Act
            using var response = await DataProcessingRegistrationV2Helper.SendPostAsync(token, request);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private string CreateEmail()
        {
            return $"{CreateName()}{DateTime.Now.Ticks}@kitos.dk";
        }

        private async Task<(string token, User user, Organization organization)> CreatePrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            return (token, user, organization);
        }
        private async Task<(User user, string token)> CreateApiUser(Organization organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Id, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userAndGetToken.userId));
            return (user, userAndGetToken.token);
        }

        private async Task<Organization> CreateOrganizationAsync(OrganizationTypeKeys orgType)
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, string.Join("", Many<int>(8).Select(x => Math.Abs(x) % 9)), orgType, AccessModifier.Public);
            return organization;
        }

        private string CreateName()
        {
            return $"{nameof(DataProcessingRegistrationApiV2Test)}{A<string>()}";
        }
    }
}
