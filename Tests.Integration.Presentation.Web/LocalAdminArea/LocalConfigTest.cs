﻿using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.LocalAdminArea
{
    public class LocalConfigTest : WithAutoFixture
    {
        [Fact]
        public async Task Cannot_Set_In_Other_Organization()
        {
            //Arrange
            var body = new
            {
                ShowItSystemModule = true
            };
            const int organizationId = TestEnvironment.SecondOrganizationId;

            //Act - perform the action with the actual role
            using var result = await LocalConfigHelper.SendUpdateConfigRequestAsync(body, organizationId);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Can_Set_It_System_Module()
        {
            //Arrange
            var body = new
            {
                ShowItSystemModule = true
            };

            //Act + Assert
            await Can_Set(body);
        }

        [Fact]
        public async Task Can_Set_It_System_Prefix()
        {
            //Arrange
            var body = new
            {
                ShowItSystemPrefix = true
            };

            //Act + Assert
            await Can_Set(body);
        }

        [Fact]
        public async Task Can_Set_It_Contract_Module()
        {
            //Arrange
            var body = new
            {
                ShowItContractModule = true
            };

            //Act + Assert
            await Can_Set(body);
        }

        [Fact]
        public async Task Can_Set_It_Contract_Prefix()
        {
            //Arrange
            var body = new
            {
                ShowItContractPrefix = true
            };

            //Act + Assert
            await Can_Set(body);
        }

        [Fact]
        public async Task Config_Values_Are_Saved()
        {
            //Arrange
            var body = new
            {
                ShowItSystemModule = A<bool>(),
                ShowItSystemPrefix = A<bool>(),
                ShowItContractModule = A<bool>(),
                ShowItContractPrefix = A<bool>()
            };
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            //Act - perform the action with the actual role
            using var result = await LocalConfigHelper.SendUpdateConfigRequestAsync(body, organizationId);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            var configResponse = await LocalConfigHelper.GetLocalConfig(organizationId);
            Assert.Equal(HttpStatusCode.OK, configResponse.StatusCode);
            var config = await configResponse.ReadResponseBodyAsAsync<Config>();
            Assert.Equal(body.ShowItSystemModule, config.ShowItSystemModule);
            Assert.Equal(body.ShowItSystemPrefix, config.ShowItSystemPrefix);
            Assert.Equal(body.ShowItContractModule, config.ShowItContractModule);
            Assert.Equal(body.ShowItContractPrefix, config.ShowItContractPrefix);
        }

        private static async Task Can_Set(object body)
        {
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            //Act - perform the action with the actual role
            using var result = await LocalConfigHelper.SendUpdateConfigRequestAsync(body, organizationId);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

    }
}
