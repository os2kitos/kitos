using System;
using Core.DomainModel.Organization;
using Moq;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
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

        private string GetCvr()
        {
            var s = A<string>();
            return s.Length <= CvrMaxLength ? s : s.Substring(0, CvrMaxLength);
        }
}
}
