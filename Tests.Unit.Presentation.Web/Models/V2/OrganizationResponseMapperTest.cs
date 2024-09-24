using System;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Moq;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class OrganizationResponseMapperTest: WithAutoFixture
    {
        private OrganizationResponseMapper _sut;
        private Mock<IOrganizationTypeMapper> _organizationTypeMapper;
        private const int CvrMaxLength = 10;

        public OrganizationResponseMapperTest()
        {
            _organizationTypeMapper = new Mock<IOrganizationTypeMapper>();
            _sut = new OrganizationResponseMapper(_organizationTypeMapper.Object);
        }

        [Fact]
        public void Can_Map_To_Organization_DTO()
        {
            var organization = new Organization()
            {
                Uuid = A<Guid>(),
                Name = A<string>(),
                Cvr = GetCvr(),
                Type = new OrganizationType(){ Id = 1 }
            };
            var expectedOrganizationType =
                global::Presentation.Web.Models.API.V2.Types.Organization.OrganizationType.Municipality;
            _organizationTypeMapper.Setup(_ => _.MapOrganizationType(organization.Type))
                .Returns(expectedOrganizationType);

            var dto = _sut.ToOrganizationDTO(organization);

            Assert.Equal(organization.Uuid, dto.Uuid);
            Assert.Equal(organization.Name, dto.Name);
            Assert.Equal(organization.Cvr, dto.Cvr);
            Assert.Equal(expectedOrganizationType, dto.OrganizationType);
        }

        [Fact]
        public void Can_Map_To_Roles_Dto()
        {
            var contactPerson = new ContactPerson()
            {
                Email = A<string>(),
                Id = A<int>(),
                Name = A<string>(),
                PhoneNumber = A<string>()
            };
            var dataResponsible = new DataResponsible()
            {
                Email = A<string>(),
                Id = A<int>(),
                Name = A<string>(),
                Phone = A<string>()
            };
            var dataProtectionAdvisor = new DataProtectionAdvisor()
            {
                Email = A<string>(),
                Id = A<int>(),
                Name = A<string>(),
                Phone = A<string>()
            };
            var orgUuid = A<Guid>();
            var roles = new OrganizationMasterDataRoles()
            {
                ContactPerson = contactPerson,
                DataResponsible = dataResponsible,
                DataProtectionAdvisor = dataProtectionAdvisor
            };

            var result = _sut.ToRolesDTO(roles);

            var cp = result.ContactPerson;
            var dr = result.DataResponsible;
            var dpa = result.DataProtectionAdvisor;
            AssertContactPerson(contactPerson, cp);
            AssertDataResponsible(dataResponsible, dr);
            AssertDataProtectionAdvisor(dataProtectionAdvisor, dpa);
        }

        private void AssertContactPerson(ContactPerson expected, ContactPersonResponseDTO actual)
        {
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.PhoneNumber, actual.PhoneNumber);
            Assert.Equal(expected.LastName, actual.LastName);
        }

        private void AssertDataResponsible(DataResponsible expected, DataResponsibleResponseDTO actual)
        {
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Cvr, actual.Cvr);
            Assert.Equal(expected.Phone, actual.Phone);
            Assert.Equal(expected.Adress, actual.Address);
        }

        private void AssertDataProtectionAdvisor(DataProtectionAdvisor expected, DataProtectionAdvisorResponseDTO actual)
        {
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Cvr, actual.Cvr);
            Assert.Equal(expected.Phone, actual.Phone);
            Assert.Equal(expected.Adress, actual.Address);
        }

        private string GetCvr()
        {
            var s = A<string>();
            return s.Length <= CvrMaxLength ? s : s.Substring(0, CvrMaxLength);
        }
}
}
