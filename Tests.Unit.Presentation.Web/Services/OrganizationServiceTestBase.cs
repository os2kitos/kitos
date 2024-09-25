using Core.DomainModel.Organization;
using Core.DomainModel;
using System.Collections.Generic;
using Tests.Toolkit.Patterns;

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
    }
}
