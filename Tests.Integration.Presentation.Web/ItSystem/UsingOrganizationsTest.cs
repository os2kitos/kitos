﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.ItSystem;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class UsingOrganizationsTest
    {
        private static readonly string NameSessionPart = Guid.NewGuid().ToString("N");
        private static long _nameCounter = 0;
        private static readonly object NameCounterLock = new object();

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task GetUsingOrganizations_Can_Get_Own_Organizations_In_Use(OrganizationRole role)
        {
            //Arrange
            var newSystem = await CreateSystemAsync();
            var newUsage = await TakeSystemIntoUseAsync(newSystem);

            //Act
            using (var httpResponse = await GetUsingOrganizations(role, newSystem.Id))
            {
                var response = await httpResponse.ReadResponseBodyAsKitosApiResponseAsync<IReadOnlyList<UsingOrganizationDTO>>();
                //Assert
                var usingOrganization = Assert.Single(response);
                Assert.Equal(newUsage.Uuid, usingOrganization.SystemUsage.Uuid);
                Assert.Equal(TestEnvironment.DefaultOrganizationId, usingOrganization.Organization.Id);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task GetUsingOrganizations_Can_Get_All_Organizations_In_Use(OrganizationRole role)
        {
            //Arrange
            var newSystem = await CreateSystemAsync(accessModifier: AccessModifier.Public);
            var firstUsage = await TakeSystemIntoUseAsync(newSystem, TestEnvironment.DefaultOrganizationId);
            var secondUsage = await TakeSystemIntoUseAsync(newSystem, TestEnvironment.SecondOrganizationId);

            //Act
            using (var httpResponse = await GetUsingOrganizations(role, newSystem.Id))
            {
                var response = await httpResponse.ReadResponseBodyAsKitosApiResponseAsync<IReadOnlyList<UsingOrganizationDTO>>();
                //Assert
                Assert.Equal(2, response.Count);
                Assert.Contains(firstUsage.Uuid, response.Select(x => x.SystemUsage.Uuid));
                Assert.Contains(secondUsage.Uuid, response.Select(x => x.SystemUsage.Uuid));
                Assert.Contains(TestEnvironment.DefaultOrganizationId, response.Select(x => x.Organization.Id));
                Assert.Contains(TestEnvironment.SecondOrganizationId, response.Select(x => x.Organization.Id));
            }
        }

        [Theory]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task GetUsingOrganizations_Non_Global_Admin_Cannot_Get_Local_Data_On_Other_Organization_Systems(OrganizationRole role)
        {
            //Arrange
            var newSystem = await CreateSystemAsync(organizationId: TestEnvironment.SecondOrganizationId);
            await TakeSystemIntoUseAsync(newSystem, TestEnvironment.SecondOrganizationId);

            //Act
            using (var httpResponse = await GetUsingOrganizations(role, newSystem.Id))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Forbidden, httpResponse.StatusCode);
            }
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        [InlineData(OrganizationRole.User)]
        public async Task GetUsingOrganizations_Cannot_Find_Non_Existing_System(OrganizationRole role)
        {
            //Act
            using (var httpResponse = await GetUsingOrganizations(role, int.MaxValue))
            {
                //Assert
                Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
            }
        }

        private static async Task<HttpResponseMessage> GetUsingOrganizations(OrganizationRole role, int itSystemId)
        {
            var cookie = await HttpApi.GetCookieAsync(role);

            var url = TestEnvironment.CreateUrl($"api/v1/ItSystem/{itSystemId}/usingOrganizations");

            return await HttpApi.GetWithCookieAsync(url, cookie);
        }

        private static Task<ItSystemDTO> CreateSystemAsync(
            int organizationId = TestEnvironment.DefaultOrganizationId,
            string name = null,
            AccessModifier accessModifier = AccessModifier.Local)
        {
            return ItSystemHelper.CreateItSystemInOrganizationAsync(name ?? CreateName(), organizationId, accessModifier);
        }

        private static async Task<ItSystemUsageDTO> TakeSystemIntoUseAsync(ItSystemDTO system, int? organizationId = null)
        {
            return await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId ?? system.OrganizationId);
        }

        private static string CreateName()
        {
            lock (NameCounterLock)
            {
                var name = $"UsingOrg_{NameSessionPart}_{_nameCounter}";
                _nameCounter++;
                return name;
            }
        }
    }
}
