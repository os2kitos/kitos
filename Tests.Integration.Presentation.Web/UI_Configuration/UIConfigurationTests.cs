
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.UI_Configuration;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Internal.UI_Configuration;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.UI_Configuration
{
    public class UIConfigurationTests : WithAutoFixture
    {
        [Fact]
        public async Task Can_Put()
        {
            var module = A<string>();
            var (cookie, organization) = await CreatePrerequisitesAsync();

            await UIConfigurationHelper.CreateUIModuleAndSaveAsync(organization.Id, module, cookie);
        }

        [Fact]
        public async Task Can_Get()
        {
            //Arrange
            var module = A<string>();
            var (cookie, organization) = await CreatePrerequisitesAsync();

            //Act
            var uiModuleCustomization = await UIConfigurationHelper.CreateUIModuleAndSaveAsync(organization.Id, module, cookie);
            
            var response = await UIConfigurationHelper.GetCustomizationByModuleAsync(organization.Id, module);

            //Assert
            var data = await response.ReadResponseBodyAsKitosApiResponseAsync<UIModuleCustomizationDTO>();
            Assert.NotNull(data);
            Assert.Single(data.Nodes);
            Assert.Single(uiModuleCustomization.Nodes);

            var responseNode = data.Nodes.FirstOrDefault();
            var uiModuleCreateNode = uiModuleCustomization.Nodes.FirstOrDefault();

            AssertDtosAreNotNullAndEqual(uiModuleCreateNode, responseNode);
        }

        [Fact]
        public async Task Get_Returns_Not_Found_If_Config_Doesnt_Exist()
        {
            //Arrange
            var module = A<string>();

            //Act
            var response = await UIConfigurationHelper.GetCustomizationByModuleAsync(TestEnvironment.DefaultOrganizationId, module);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_Returns_Forbidden_If_Not_Allowed_To_Read()
        {
            //Arrange
            var module = A<string>();
            var (cookie, organization) = await CreatePrerequisitesAsync();
            var (cookieUser2, _) = await CreatePrerequisitesAsync();

            await UIConfigurationHelper.CreateUIModuleAndSaveAsync(organization.Id, module, cookie);

            //Act
            using var response = await UIConfigurationHelper.SendGetRequestAsync(organization.Id, module, cookieUser2);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden ,response.StatusCode);
        }

        [Fact]
        public async Task Put_Deletes_Nodes()
        {
            var module = A<string>();
            var multipleUiCustomizations= UIConfigurationHelper.PrepareTestUiModuleCustomizationDto(10);
            var singleUiCustomization = UIConfigurationHelper.PrepareTestUiModuleCustomizationDto();
            var (cookie, organization) = await CreatePrerequisitesAsync();

            using var firstPutResponse = await UIConfigurationHelper.SendPutRequestAsync(organization.Id, module, multipleUiCustomizations, cookie);
            Assert.Equal(HttpStatusCode.NoContent, firstPutResponse.StatusCode);

            using var secondPutResponse = await UIConfigurationHelper.SendPutRequestAsync(organization.Id, module, singleUiCustomization, cookie);
            Assert.Equal(HttpStatusCode.NoContent, secondPutResponse.StatusCode);

            var response = await UIConfigurationHelper.GetCustomizationByModuleAsync(organization.Id, module);
            var data = await response.ReadResponseBodyAsKitosApiResponseAsync<UIModuleCustomizationDTO>();

            Assert.Equal(singleUiCustomization.Nodes.Count(), data.Nodes.Count());
            Assert.Single(data.Nodes);
            
            var responseNode = data.Nodes.FirstOrDefault();
            var singleUiNode = singleUiCustomization.Nodes.FirstOrDefault();

            AssertDtosAreNotNullAndEqual(singleUiNode, responseNode);
        }

        [Fact]
        public async Task Put_Updates_Existing_Nodes()
        {
            var module = A<string>();
            var uiCustomizations= UIConfigurationHelper.PrepareTestUiModuleCustomizationDto(3, "", true);
            var (cookie, organization) = await CreatePrerequisitesAsync();

            using var firstPutResponse = await UIConfigurationHelper.SendPutRequestAsync(organization.Id, module, uiCustomizations, cookie);
            Assert.Equal(HttpStatusCode.NoContent, firstPutResponse.StatusCode);

            foreach (var node in uiCustomizations.Nodes)
            {
                node.Enabled = false;
            }

            using var secondPutResponse = await UIConfigurationHelper.SendPutRequestAsync(organization.Id, module, uiCustomizations, cookie);
            Assert.Equal(HttpStatusCode.NoContent, secondPutResponse.StatusCode);

            var response = await UIConfigurationHelper.GetCustomizationByModuleAsync(organization.Id, module);
            var data = await response.ReadResponseBodyAsKitosApiResponseAsync<UIModuleCustomizationDTO>();

            Assert.Equal(uiCustomizations.Nodes.Count(), data.Nodes.Count());

            Assert.True(data.Nodes.All(x => !x.Enabled));
        }

        [Fact]
        public async Task Put_Returns_BadRequest_If_Keys_Are_Invalid()
        {
            //Arrange
            var module = A<string>();
            var incorrectKey = "Incorrect_Key123";
            var uiModuleCustomizationDto = UIConfigurationHelper.PrepareTestUiModuleCustomizationDto(1, incorrectKey);
            var (cookie, organization) = await CreatePrerequisitesAsync();

            //Act
            using var putResponse = await UIConfigurationHelper.SendPutRequestAsync(organization.Id, module, uiModuleCustomizationDto, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
        }

        [Fact]
        public async Task Put_Returns_NotFound_If_Organization_Doesnt_Exist()
        {
            //Arrange
            var module = A<string>();
            var incorrectOrgId = -1;
            var uiModuleCustomizationDto = UIConfigurationHelper.PrepareTestUiModuleCustomizationDto();
            var (cookie, _) = await CreatePrerequisitesAsync();

            //Act
            using var putResponse = await UIConfigurationHelper.SendPutRequestAsync(incorrectOrgId, module, uiModuleCustomizationDto, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, putResponse.StatusCode);
        }

        [Fact]
        public async Task Forbidden_If_User_Is_Not_Local_Admin_In_The_Target_Organization()
        {
            var module = A<string>();
            var uiModuleCustomizationDto = UIConfigurationHelper.PrepareTestUiModuleCustomizationDto();
            var (cookie, organization) = await CreatePrerequisitesAsync();
            var (_, _, sameOrgCookie) = await HttpApi.CreateUserAndLogin(UIConfigurationHelper.CreateEmail(), OrganizationRole.User, organization.Id);
            
            using var otherOrgPutResponse = await UIConfigurationHelper.SendPutRequestAsync(TestEnvironment.DefaultOrganizationId, module, uiModuleCustomizationDto, cookie);
            Assert.Equal(HttpStatusCode.Forbidden, otherOrgPutResponse.StatusCode);

            using var sameOrgPutResponse = await UIConfigurationHelper.SendPutRequestAsync(organization.Id, module, uiModuleCustomizationDto, sameOrgCookie);
            Assert.Equal(HttpStatusCode.Forbidden, sameOrgPutResponse.StatusCode);
        }
        
        private async Task<(Cookie loginCookie, OrganizationDTO organization)> CreatePrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (_, _, loginCookie) =
                await HttpApi.CreateUserAndLogin(UIConfigurationHelper.CreateEmail(), OrganizationRole.LocalAdmin, organization.Id);
            return (loginCookie, organization);
        }

        private void AssertDtosAreNotNullAndEqual(CustomizedUINodeDTO dto1, CustomizedUINodeDTO dto2)
        {
            Assert.NotNull(dto1);
            Assert.NotNull(dto2);
            Assert.Equal(dto1.Enabled, dto2.Enabled);
            Assert.Equal(dto1.Key, dto2.Key);
        }

        private async Task<OrganizationDTO> CreateOrganizationAsync(OrganizationTypeKeys orgType)
        {
            var organizationName = UIConfigurationHelper.CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, string.Join("", Many<int>(8).Select(x => Math.Abs(x) % 9)), orgType, AccessModifier.Public);
            return organization;
        }
    }
}
