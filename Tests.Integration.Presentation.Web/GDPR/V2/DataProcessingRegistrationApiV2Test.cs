using Core.DomainModel;
using Core.DomainModel.Organization;
using System;
using System.Net;
using System.Threading.Tasks;
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

        private string CreateName()
        {
            return $"{nameof(DataProcessingRegistrationApiV2Test)}{A<string>()}";
        }
    }
}
