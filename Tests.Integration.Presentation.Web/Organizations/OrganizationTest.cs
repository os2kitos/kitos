﻿using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations
{
    public class OrganizationTest : WithAutoFixture
    {
        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin)]
        [InlineData(OrganizationRole.LocalAdmin)]
        public async Task Can_Set_ContactPerson(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var contactPersonDto = await OrganizationHelper.GetContactPersonAsync(organizationId);
            var name = A<string>();
            var lastName = A<string>();
            var email = A<string>();
            var phone = A<string>();

            //Act - perform the action with the actual role
            var result = await OrganizationHelper.ChangeContactPersonAsync(contactPersonDto.Id, organizationId, name, lastName, email, phone, login);

            //Assert
            Assert.Equal(contactPersonDto.Id, result.Id);
            Assert.Equal(email, result.Email);
            Assert.Equal(name, result.Name);
            Assert.Equal(lastName, result.LastName);
            Assert.Equal(phone, result.PhoneNumber);
            Assert.Equal(organizationId, result.OrganizationId);
        }

        [Theory]
        [InlineData(OrganizationRole.User)]
        public async Task Cannot_Set_ContactPerson(OrganizationRole role)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var contactPersonDto = await OrganizationHelper.GetContactPersonAsync(organizationId);
            var name = A<string>();
            var lastName = A<string>();
            var email = A<string>();
            var phone = A<string>();

            //Act - perform the action with the actual role
            using var result = await OrganizationHelper.SendChangeContactPersonRequestAsync(contactPersonDto.Id, organizationId, name, lastName, email, phone, login);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationTypeKeys.AndenOffentligMyndighed)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationTypeKeys.Interessefællesskab)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationTypeKeys.Kommune)]
        [InlineData(OrganizationRole.GlobalAdmin, OrganizationTypeKeys.Virksomhed)]
        public async Task Can_Create_Organization_Of_Type(OrganizationRole role, OrganizationTypeKeys organizationType)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            var name = A<string>();
            var cvr = (A<int>() % 9999999999).ToString("D10");
            const AccessModifier accessModifier = AccessModifier.Public;

            //Act - perform the action with the actual role
            var result = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, name, cvr, organizationType, accessModifier, login);

            //Assert
            Assert.Equal(accessModifier, result.AccessModifier);
            Assert.Equal(name, (string)result.Name);
            Assert.Equal(cvr, (string)result.Cvr);
        }

        [Theory]
        [InlineData(OrganizationRole.User, OrganizationTypeKeys.AndenOffentligMyndighed)]
        [InlineData(OrganizationRole.User, OrganizationTypeKeys.Interessefællesskab)]
        [InlineData(OrganizationRole.User, OrganizationTypeKeys.Kommune)]
        [InlineData(OrganizationRole.User, OrganizationTypeKeys.Virksomhed)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationTypeKeys.Kommune)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationTypeKeys.AndenOffentligMyndighed)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationTypeKeys.Interessefællesskab)]
        [InlineData(OrganizationRole.LocalAdmin, OrganizationTypeKeys.Virksomhed)]
        public async Task Cannot_Create_Organization_Of_Type(OrganizationRole role, OrganizationTypeKeys organizationType)
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(role);
            var name = A<string>();
            var cvr = (A<int>() % 9999999999).ToString("D10");
            const AccessModifier accessModifier = AccessModifier.Public;

            //Act - perform the action with the actual role
            using var result = await OrganizationHelper.SendCreateOrganizationRequestAsync(TestEnvironment.DefaultOrganizationId, name, cvr, organizationType, accessModifier, login);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Can_Get_Organizations_Filtered_By_Cvr_Or_Name()
        {
            //Arrange
            var login = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var nameOrg1 = A<string>();
            var cvrOrg1 = (A<int>() % 9999999999).ToString("D10");
            const AccessModifier accessModifier = AccessModifier.Public;

            //Act - perform the action with the actual role
            using var _ = await OrganizationHelper.SendCreateOrganizationRequestAsync(TestEnvironment.DefaultOrganizationId, nameOrg1, cvrOrg1, OrganizationTypeKeys.Kommune, accessModifier, login);

            using var organizationsFilteredByCvr = await OrganizationHelper.SendGetOrganizationSearchRequestAsync(cvrOrg1);
            Assert.True(organizationsFilteredByCvr.IsSuccessStatusCode);

            var resultFilteredByCvr = await organizationsFilteredByCvr.ReadResponseBodyAsKitosApiResponseAsync<List<Organization>>();
            Assert.True(resultFilteredByCvr.Exists(prp => prp.Cvr.Contains(cvrOrg1)));

            using var organizationsFilteredByName = await OrganizationHelper.SendGetOrganizationSearchRequestAsync(nameOrg1);
            Assert.True(organizationsFilteredByName.IsSuccessStatusCode);

            var resultFilteredByName = await organizationsFilteredByName.ReadResponseBodyAsKitosApiResponseAsync<List<Organization>>();
            Assert.True(resultFilteredByName.Exists(prp => prp.Name.Contains(nameOrg1)));
        }
    }
}
