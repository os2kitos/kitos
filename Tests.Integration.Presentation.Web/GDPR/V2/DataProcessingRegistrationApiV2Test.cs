using Core.DomainModel;
using Core.DomainModel.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Shared;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V1.GDPR;
using Presentation.Web.Models.API.V2.Response.DataProcessing;
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
        public async Task Can_GET_Specific_DPR()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var newDPR = await CreateDPRAsync(organization.Id);

            //Act
            var dto = await DataProcessingRegistrationV2Helper.GetDPRAsync(token, newDPR.Uuid);

            //Assert
            AssertExpectedShallowDPR(newDPR, organization, dto);
        }

        [Fact]
        public async Task Cannot_Get_DPR_If_Unknown()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await DataProcessingRegistrationV2Helper.SendGetDPRAsync(regularUserToken.Token, A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Get_DPR_If_NotAllowedTo()
        {
            //Arrange
            var (token, user, organization1) = await CreatePrerequisitesAsync();
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var newDPR = await CreateDPRAsync(organization2.Id);

            //Act
            using var response = await DataProcessingRegistrationV2Helper.SendGetDPRAsync(token, newDPR.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Get_DPR_If_Empty_Uuid_In_Request()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            //Act
            using var response = await DataProcessingRegistrationV2Helper.SendGetDPRAsync(token, Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_GET_All_DPRs()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 100);

            //Assert
            Assert.Equal(2, dprs.Count());
            AssertExpectedShallowDPRs(dpr1, organization, dprs);
            AssertExpectedShallowDPRs(dpr2, organization, dprs);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_Paging()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            var dpr3 = await CreateDPRAsync(organization.Id);

            //Act
            var page1Dprs = (await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 2)).ToList();
            var page2Dprs = (await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 1, 2)).ToList();

            //Assert
            Assert.Equal(2, page1Dprs.Count());
            AssertExpectedShallowDPRs(dpr1, organization, page1Dprs);
            AssertExpectedShallowDPRs(dpr2, organization, page1Dprs);

            var page2Dpr = Assert.Single(page2Dprs);
            AssertExpectedShallowDPR(dpr3, organization, page2Dpr);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_OrganizationFiltering()
        {
            //Arrange
            var (token, user, organization1) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization1.Id);
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var dpr2 = await CreateDPRAsync(organization2.Id);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 100);

            //Assert
            var retrievedDPR = Assert.Single(dprs);
            AssertExpectedShallowDPR(dpr1, organization1, retrievedDPR);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_SystemFiltering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var newSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Local);
            var newSystemUsage = await ItSystemHelper.TakeIntoUseAsync(newSystem.Id, organization.Id);
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            using var assignResult = await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(dpr1.Id, newSystemUsage.Id);
            Assert.Equal(HttpStatusCode.OK, assignResult.StatusCode);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 100, systemUuid: newSystem.Uuid);

            //Assert
            var retrievedDPR = Assert.Single(dprs);
            AssertExpectedShallowDPR(dpr1, organization, retrievedDPR);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_SystemUsageFiltering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var newSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organization.Id, AccessModifier.Local);
            var newSystemUsage = await ItSystemHelper.TakeIntoUseAsync(newSystem.Id, organization.Id);
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            using var assignResult = await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(dpr1.Id, newSystemUsage.Id);
            Assert.Equal(HttpStatusCode.OK, assignResult.StatusCode);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 100, systemUsageUuid: newSystemUsage.Uuid);

            //Assert
            var retrievedDPR = Assert.Single(dprs);
            AssertExpectedShallowDPR(dpr1, organization, retrievedDPR);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_DataProcessorFiltering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dataProcessor = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            using var assignResult = await DataProcessingRegistrationHelper.SendAssignDataProcessorRequestAsync(dpr1.Id, dataProcessor.Id);
            Assert.Equal(HttpStatusCode.OK, assignResult.StatusCode);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 100, dataProcessorUuid: dataProcessor.Uuid);

            //Assert
            var retrievedDPR = Assert.Single(dprs);
            AssertExpectedShallowDPR(dpr1, organization, retrievedDPR);
        }

        [Fact]
        public async Task Can_GET_All_DPRs_With_SubDataProcessorFiltering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var subDataProcessor = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            using var setStateResult = await DataProcessingRegistrationHelper.SendSetUseSubDataProcessorsStateRequestAsync(dpr1.Id,YesNoUndecidedOption.Yes);
            Assert.Equal(HttpStatusCode.OK, setStateResult.StatusCode);
            using var assignResult = await DataProcessingRegistrationHelper.SendAssignSubDataProcessorRequestAsync(dpr1.Id, subDataProcessor.Id);
            Assert.Equal(HttpStatusCode.OK, assignResult.StatusCode);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 100, subDataProcessorUuid: subDataProcessor.Uuid);

            //Assert
            var retrievedDPR = Assert.Single(dprs);
            AssertExpectedShallowDPR(dpr1, organization, retrievedDPR);
        }

        [Theory]
        [InlineData(YesNoIrrelevantOption.YES, YesNoIrrelevantChoice.Yes)]
        [InlineData(YesNoIrrelevantOption.NO, YesNoIrrelevantChoice.No)]
        [InlineData(YesNoIrrelevantOption.IRRELEVANT, YesNoIrrelevantChoice.Irrelevant)]
        [InlineData(YesNoIrrelevantOption.UNDECIDED, YesNoIrrelevantChoice.Undecided)]
        public async Task Can_GET_All_DPRs_With_AgreementConcludedFiltering(YesNoIrrelevantOption optionValue, YesNoIrrelevantChoice choiceValue)
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();
            var dpr1 = await CreateDPRAsync(organization.Id);
            var dpr2 = await CreateDPRAsync(organization.Id);
            using var assignResult = await DataProcessingRegistrationHelper.SendChangeIsAgreementConcludedRequestAsync(dpr1.Id, optionValue);
            Assert.Equal(HttpStatusCode.OK, assignResult.StatusCode);

            //Act
            var dprs = await DataProcessingRegistrationV2Helper.GetDPRsAsync(token, 0, 100, agreementConcluded: choiceValue);

            //Assert
            var retrievedDPR = Assert.Single(dprs);
            AssertExpectedShallowDPR(dpr1, organization, retrievedDPR);
        }

        [Fact]
        public async Task Cannot_Get_All_Dprs_If_Empty_Guid_Used_For_Filtering()
        {
            //Arrange
            var (token, user, organization) = await CreatePrerequisitesAsync();

            //Act
            using var getResult = await DataProcessingRegistrationV2Helper.SendGetDPRsAsync(token, 0, 100, organizationUuid: Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, getResult.StatusCode);
        }

        #region Asserters

        private static void AssertExpectedShallowDPRs(DataProcessingRegistrationDTO expectedContent, Organization expectedOrganization, IEnumerable<DataProcessingRegistrationResponseDTO> dtos)
        {
            var dto = Assert.Single(dtos, dpr => dpr.Uuid == expectedContent.Uuid);
            AssertExpectedShallowDPR(expectedContent, expectedOrganization, dto);
        }

        private static void AssertExpectedShallowDPR(DataProcessingRegistrationDTO expectedContent, Organization expectedOrganization, DataProcessingRegistrationResponseDTO dto)
        {
            Assert.Equal(expectedContent.Uuid, dto.Uuid);
            Assert.Equal(expectedContent.Name, dto.Name);
            Assert.Equal(expectedOrganization.Uuid, dto.OrganizationContext.Uuid);
            Assert.Equal(expectedOrganization.Name, dto.OrganizationContext.Name);
            Assert.Equal(expectedOrganization.Cvr, dto.OrganizationContext.Cvr);
        }

        #endregion

        #region Creaters

        private async Task<DataProcessingRegistrationDTO> CreateDPRAsync(int orgId)
        {
            return await DataProcessingRegistrationHelper.CreateAsync(orgId, CreateName());
        }

        private async Task<(string token, User user, Organization organization)> CreatePrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUserAsync(organization);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization.Id).DisposeAsync();
            return (token, user, organization);
        }

        private async Task<(User user, string token)> CreateApiUserAsync(Organization organization)
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
        private string CreateEmail()
        {
            return $"{CreateName()}{DateTime.Now.Ticks}@kitos.dk";
        }

        #endregion
    }
}
