using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
            //Arrange
            var module = A<string>();

            var body = PrepareTestUiModuleCustomizationDto();

            //Act
            using var response = await UIConfigurationHelper.SendPutRequestAsync(TestEnvironment.DefaultOrganizationId, module, body);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Can_Get()
        {
            //Arrange
            var module = A<string>();
            var uiModuleCustomizationDto = PrepareTestUiModuleCustomizationDto();

            //Act
            using var putResponse = await UIConfigurationHelper.SendPutRequestAsync(TestEnvironment.DefaultOrganizationId, module, uiModuleCustomizationDto);
            Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

            using var getNodulesresponse = await UIConfigurationHelper.SendGetRequestAsync(TestEnvironment.DefaultOrganizationId, module);

            //Assert
            Assert.Equal(HttpStatusCode.OK, getNodulesresponse.StatusCode);

            var data = await getNodulesresponse.ReadResponseBodyAsKitosApiResponseAsync<List<UIModuleCustomizationDTO>>();
            Assert.NotNull(data);
            Assert.Single(data);
        }

        private UIModuleCustomizationDTO PrepareTestUiModuleCustomizationDto()
        {
            var key = "Module.Key";
            var isEnabled = false;

            return new UIModuleCustomizationDTO
            {
                Nodes = new List<CustomizedUINodeDTO>
                {
                    new()
                    {
                        Key = key,
                        Enabled = isEnabled
                    }
                }
            };
        }
    }
}
