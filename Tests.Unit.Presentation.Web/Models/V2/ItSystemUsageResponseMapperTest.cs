using System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.Organization;
using Moq;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItSystemUsageResponseMapperTest : WithAutoFixture
    {
        private readonly ItSystemUsageResponseMapper _sut;
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly Mock<IAttachedOptionRepository> _attachedOptionsRepositoryMock;
        private readonly Mock<ISensitivePersonalDataTypeRepository> _sensitivePersonalDataTypeRepositoryMock;
        private readonly Mock<IGenericRepository<RegisterType>> _registerTypeRepositoryMock;

        public ItSystemUsageResponseMapperTest()
        {
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _attachedOptionsRepositoryMock = new Mock<IAttachedOptionRepository>();
            _sensitivePersonalDataTypeRepositoryMock = new Mock<ISensitivePersonalDataTypeRepository>();
            _registerTypeRepositoryMock = new Mock<IGenericRepository<RegisterType>>();
            _sut = new ItSystemUsageResponseMapper(
                _organizationRepositoryMock.Object,
                _attachedOptionsRepositoryMock.Object,
                _sensitivePersonalDataTypeRepositoryMock.Object,
                _registerTypeRepositoryMock.Object
                );
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_Basic_Properties()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage
            {
                LastChanged = A<DateTime>(),
                Uuid = A<Guid>(),
                ObjectOwner = CreateUser(),
                LastChangedByUser = CreateUser(),
                ItSystem = CreateSystem(),
                Organization = CreateOrganization()
            };

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            Assert.Equal(itSystemUsage.Uuid, dto.Uuid);
            Assert.Equal(itSystemUsage.LastChanged, dto.LastModified);
            AssertUser(itSystemUsage.ObjectOwner, dto.CreatedBy);
            AssertUser(itSystemUsage.LastChangedByUser, dto.LastModifiedBy);
            AssertIdentity(itSystemUsage.ItSystem, dto.SystemContext);
            AssertOrganization(itSystemUsage.Organization, dto.OrganizationContext);

        }

        private void AssertOrganization(Organization organization, ShallowOrganizationResponseDTO dtoOrganizationContext)
        {
            AssertIdentity(organization, dtoOrganizationContext);
            Assert.Equal(organization.Cvr, dtoOrganizationContext.Cvr);
        }

        private static void AssertIdentity<T>(T sourceIdentity, IdentityNamePairResponseDTO dto) where T : IHasUuid, IHasName
        {
            Assert.Equal(sourceIdentity.Name, dto.Name);
            Assert.Equal(sourceIdentity.Uuid, dto.Uuid);
        }

        private Organization CreateOrganization()
        {
            return new Organization { Name = A<string>(), Cvr = A<string>(), Uuid = A<Guid>() };
        }

        private ItSystem CreateSystem()
        {
            return new ItSystem
            {
                Uuid = A<Guid>(),
                Name = A<string>()
            };
        }

        private static void AssertUser(User user, IdentityNamePairResponseDTO dtoValue)
        {
            Assert.Equal((user.GetFullName(), user.Uuid), (dtoValue.Name, dtoValue.Uuid));
        }

        private User CreateUser()
        {
            return new User
            {
                Name = A<string>(),
                LastName = A<string>(),
                Uuid = A<Guid>()
            };
        }
    }
}
