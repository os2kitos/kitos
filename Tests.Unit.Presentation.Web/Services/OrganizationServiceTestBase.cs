using Core.DomainModel.Organization;
using Core.DomainModel;
using Tests.Toolkit.Patterns;
using Xunit;
using System;

namespace Tests.Unit.Presentation.Web.Services
{
    public class OrganizationServiceTestBase: WithAutoFixture
    {
        protected DataProtectionAdvisor SetupGetMasterDataRolesDataProtectionAdvisor(int orgId)
        {
            var expectedDataProtectionAdvisor = new DataProtectionAdvisor()
            {
                Email = A<string>(),
                Name = A<string>(),
                Cvr = A<string>(),
                Adress = A<string>(),
                OrganizationId = orgId,
                Phone = A<string>(),
                Id = A<int>(),
            };
             return expectedDataProtectionAdvisor;
        }

        protected ContactPerson SetupGetMasterDataRolesContactPerson(int orgId)
        {
            var expectedContactPerson = new ContactPerson
            {
                Email = A<string>(),
                Name = A<string>(),
                PhoneNumber = A<string>(),
                OrganizationId = orgId,
                Id = A<int>(),
            };
             return expectedContactPerson;
        }

        protected DataResponsible SetupGetMasterDataRolesDataResponsible(int orgId)
        {
            var expectedDataResponsible = new DataResponsible
            {
                Email = A<string>(),
                Name = A<string>(),
                Cvr = A<string>(),
                Adress = A<string>(),
                Phone = A<string>(),
                OrganizationId = orgId,
                Id = A<int>(),
            };
            return expectedDataResponsible;
        }

        protected void AssertContactPerson(ContactPerson expected, ContactPerson actual)
        {
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.PhoneNumber, actual.PhoneNumber);
            Assert.Equal(expected.OrganizationId, actual.OrganizationId);
        }

        protected void AssertDataResponsible(DataResponsible expected, DataResponsible actual)
        {
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Cvr, actual.Cvr);
            Assert.Equal(expected.OrganizationId, actual.OrganizationId);
            Assert.Equal(expected.Adress, actual.Adress);
        }

        protected void AssertDataProtectionAdvisor(DataProtectionAdvisor expected, DataProtectionAdvisor actual)
        {
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Cvr, actual.Cvr);
            Assert.Equal(expected.OrganizationId, actual.OrganizationId);
            Assert.Equal(expected.Adress, actual.Adress);
        }
        protected Organization CreateOrganization()
        {
            var organizationId = A<Guid>();
            var organization = new Organization() { Uuid = organizationId, Id = A<int>() };
            return organization;
        }
    }
}
