﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Kendo
{
    public class KendoOverviewConfigurationTest : WithAutoFixture
    {
        private const int IdOfNonExistingOrg = int.MaxValue;

        public static IEnumerable<object[]> GetOverviewTypes()
        {
            return EnumRange.All<OverviewType>().Select(x => new object[] { x });
        }

        [Theory, MemberData(nameof(GetOverviewTypes))]
        public async Task Can_Save_Configuration(OverviewType overviewType)
        {
            //Arrange
            var columns = CreateColumnConfigurations();

            //Act
            using var response = await KendoOverviewConfigurationHelper.SendSaveConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType, columns);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var kendoConfig = await response.ReadResponseBodyAsKitosApiResponseAsync<KendoOrganizationalConfigurationDTO>();
            Assert.Equal(TestEnvironment.DefaultOrganizationId, kendoConfig.OrganizationId);
            Assert.Equal(overviewType, kendoConfig.OverviewType);
            columns.ForEach(x =>
            {
                Assert.Contains(x.PersistId, kendoConfig.VisibleColumns.Select(y => y.PersistId));
            });
        }

        [Theory, MemberData(nameof(GetOverviewTypes))]
        public async Task Can_Update_Configuration(OverviewType overviewType)
        {
            //Arrange
            var columns = CreateColumnConfigurations();
            using var response = await KendoOverviewConfigurationHelper.SendSaveConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType, columns);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var kendoConfig = await response.ReadResponseBodyAsKitosApiResponseAsync<KendoOrganizationalConfigurationDTO>();
            Assert.Equal(TestEnvironment.DefaultOrganizationId, kendoConfig.OrganizationId);
            Assert.Equal(overviewType, kendoConfig.OverviewType);
            columns.ForEach(x =>
            {
                Assert.Contains(x.PersistId, kendoConfig.VisibleColumns.Select(y => y.PersistId));
            });

            var newColumns = CreateColumnConfigurations();

            //Act
            using var updateResponse = await KendoOverviewConfigurationHelper.SendSaveConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType, newColumns);

            //Assert
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            var updatedKendoConfig = await updateResponse.ReadResponseBodyAsKitosApiResponseAsync<KendoOrganizationalConfigurationDTO>();
            Assert.Equal(TestEnvironment.DefaultOrganizationId, updatedKendoConfig.OrganizationId);
            Assert.Equal(overviewType, updatedKendoConfig.OverviewType);
            newColumns.ForEach(x =>
            {
                Assert.Contains(x.PersistId, updatedKendoConfig.VisibleColumns.Select(y => y.PersistId));
            });

        }

        [Theory, MemberData(nameof(GetOverviewTypes))]
        public async Task Can_Get_Configuration(OverviewType overviewType)
        {
            //Arrange
            var columns = CreateColumnConfigurations();

            using var saveResponse = await KendoOverviewConfigurationHelper.SendSaveConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType, columns);
            Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

            //Act
            using var response = await KendoOverviewConfigurationHelper.SendGetConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var kendoConfig = await response.ReadResponseBodyAsKitosApiResponseAsync<KendoOrganizationalConfigurationDTO>();
            Assert.Equal(TestEnvironment.DefaultOrganizationId, kendoConfig.OrganizationId);
            Assert.Equal(overviewType, kendoConfig.OverviewType);
            columns.ForEach(x =>
            {
                Assert.Contains(x.PersistId, kendoConfig.VisibleColumns.Select(y => y.PersistId));
            });
        }

        [Theory, MemberData(nameof(GetOverviewTypes))]
        public async Task Can_Delete_Configuration(OverviewType overviewType)
        {
            //Arrange
            using var saveResponse = await KendoOverviewConfigurationHelper.SendSaveConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType, CreateColumnConfigurations());
            Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

            //Act
            using var response = await KendoOverviewConfigurationHelper.SendDeleteConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory, MemberData(nameof(GetOverviewTypes))]
        public async Task Can_Not_Get_Configuration_If_None_Exists(OverviewType overviewType)
        {
            //Arrange
            var orgId = IdOfNonExistingOrg;

            //Act
            using var response = await KendoOverviewConfigurationHelper.SendGetConfigurationRequestAsync(orgId, overviewType);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        public async Task Can_Not_Save_Configuration_If_Not_Allowed(OrganizationRole orgRole)
        {
            //Arrange
            var overviewType = A<OverviewType>();
            var columns = CreateColumnConfigurations();
            var cookie = await HttpApi.GetCookieAsync(orgRole);

            //Act
            using var response = await KendoOverviewConfigurationHelper.SendSaveConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType, columns, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory, MemberData(nameof(GetOverviewTypes))]
        public async Task Can_Not_Delete_Configuration_If_None_Exists(OverviewType overviewType)
        {
            //Arrange
            var orgId = IdOfNonExistingOrg;

            //Act
            using var response = await KendoOverviewConfigurationHelper.SendDeleteConfigurationRequestAsync(orgId, overviewType);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        public async Task Can_Not_Delete_Configuration_If_Not_Allowed(OrganizationRole orgRole)
        {
            //Arrange
            var overviewType = A<OverviewType>();
            using var saveResponse = await KendoOverviewConfigurationHelper.SendSaveConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType, CreateColumnConfigurations());
            Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

            var cookie = await HttpApi.GetCookieAsync(orgRole);

            //Act
            using var response = await KendoOverviewConfigurationHelper.SendDeleteConfigurationRequestAsync(TestEnvironment.DefaultOrganizationId, overviewType, cookie);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        public List<KendoColumnConfigurationDTO> CreateColumnConfigurations()
        {
            return new List<KendoColumnConfigurationDTO>
            {
                new()
                {
                    PersistId = A<string>()
                },
                new()
                {
                    PersistId = A<string>()
                }
            };
        }
    }
}
