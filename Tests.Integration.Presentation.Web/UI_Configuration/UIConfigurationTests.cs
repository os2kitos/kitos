using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
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

            await UIConfigurationHelper.CreateUIModuleAndSaveAsync(TestEnvironment.DefaultOrganizationId, module);
        }

        [Fact]
        public async Task Can_Get()
        {
            //Arrange
            var module = A<string>();
            
            //Act
            await UIConfigurationHelper.CreateUIModuleAndSaveAsync(TestEnvironment.DefaultOrganizationId, module);
            
            var data = await UIConfigurationHelper.GetAllAsync(TestEnvironment.DefaultOrganizationId, module);

            //Assert
            Assert.NotNull(data);
            Assert.Single(data);
        }

        [Fact]
        public async Task Get_Returns_NotFound_If_Config_Doesnt_Exist()
        {
            //Arrange
            var module = A<string>();

            //Act
            using var response = await UIConfigurationHelper.SendGetRequestAsync(TestEnvironment.DefaultOrganizationId, module);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound ,response.StatusCode);
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
        public async Task Put_Returns_BadRequest_If_Keys_Are_Invalid()
        {
            //Arrange
            var module = A<string>();
            var incorrectKey = "Incorrect_Key123";
            var uiModuleCustomizationDto = UIConfigurationHelper.PrepareTestUiModuleCustomizationDto(1, incorrectKey);

            //Act
            using var putResponse = await UIConfigurationHelper.SendPutRequestAsync(TestEnvironment.DefaultOrganizationId, module, uiModuleCustomizationDto);

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

            //Act
            using var putResponse = await UIConfigurationHelper.SendPutRequestAsync(incorrectOrgId, module, uiModuleCustomizationDto);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, putResponse.StatusCode);
        }

        [Fact]
        public async Task Put_Returns_Forbidden_If_Unauthorized()
        {
            //Arrange
            var module = A<string>();
            var uiModuleCustomizationDto = UIConfigurationHelper.PrepareTestUiModuleCustomizationDto();
            var (cookie, organization) = await CreatePrerequisitesAsync();

            //Act
            using var putResponse = await UIConfigurationHelper.SendPutRequestAsync(TestEnvironment.DefaultOrganizationId, module, uiModuleCustomizationDto, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, putResponse.StatusCode);
        }
        
        private async Task<(Cookie loginCookie, OrganizationDTO organization)> CreatePrerequisitesAsync()
        {
            var organization = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (_, _, loginCookie) =
                await HttpApi.CreateUserAndLogin(UIConfigurationHelper.CreateEmail(), OrganizationRole.LocalAdmin, organization.Id);
            return (loginCookie, organization);
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
