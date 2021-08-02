using Core.DomainModel;
using Core.DomainModel.Organization;
using System.Collections.Generic;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Model
{
    public class UserTest : WithAutoFixture
    {
        [Fact]
        public void GetOrganizationNames_Returns_OrganizationNames()
        {
            //Arrange
            var organization = CreateOrganization();
            var user = new User()
            {
                Id = A<int>()
            };
            var organizationRights = new List<OrganizationRight>()
            { 
                CreateOrgRight(organization, user)
            };

            user.OrganizationRights = organizationRights;

            //Act
            var orgNamesResult = user.GetOrganizationNames();

            //Assert
            var orgNameResult = Assert.Single(orgNamesResult);

            Assert.Equal(organization.Name, orgNameResult);
        }

        [Fact]
        public void GetOrganizationNames_Returns_Only_Distinct_OrganizationNames()
        {
            //Arrange
            Organization organization1 = CreateOrganization();
            var organization2 = CreateOrganization();
            var user = new User()
            {
                Id = A<int>()
            };
            var organizationRights = new List<OrganizationRight>()
            {
                CreateOrgRight(organization1, user),
                CreateOrgRight(organization1, user),
                CreateOrgRight(organization2, user)
            };

            user.OrganizationRights = organizationRights;

            //Act
            var orgNamesResult = user.GetOrganizationNames();

            //Assert
            Assert.Equal(2, orgNamesResult.Count());

            Assert.Equal(organization1.Name, orgNamesResult.First(x => x.Equals(organization1.Name)));
            Assert.Equal(organization2.Name, orgNamesResult.First(x => x.Equals(organization2.Name)));
        }

        private Organization CreateOrganization()
        {
            return new Organization()
            {
                Name = A<string>(),
                Id = A<int>()
            };
        }

        private static OrganizationRight CreateOrgRight(Organization organization, User user)
        {
            return new OrganizationRight()
            {
                OrganizationId = organization.Id,
                Organization = organization,

                UserId = user.Id,
                User = user,
            };
        }
    }
}
