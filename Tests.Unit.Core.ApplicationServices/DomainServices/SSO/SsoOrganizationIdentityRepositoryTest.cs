using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.SSO;
using Core.DomainServices;
using Core.DomainServices.Repositories.SSO;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.SSO
{
    public class SsoOrganizationIdentityRepositoryTest : WithAutoFixture
    {
        private readonly Mock<IGenericRepository<SsoOrganizationIdentity>> _repository;
        private readonly SsoOrganizationIdentityRepository _sut;

        public SsoOrganizationIdentityRepositoryTest()
        {
            _repository = new Mock<IGenericRepository<SsoOrganizationIdentity>>();
            _sut = new SsoOrganizationIdentityRepository(_repository.Object);
        }

        [Fact]
        public void GetByExternalUuid_Returns_Value()
        {
            //Arrange
            var inputId = A<Guid>();
            var expectedResponse = CreateSsoOrganizationIdentity(inputId);

            ExpectRepositoryContent(CreateSsoOrganizationIdentity(), expectedResponse);

            //Act
            var result = _sut.GetByExternalUuid(inputId);

            //Assert
            Assert.True(result.HasValue);
            Assert.Same(expectedResponse, result.Value);
        }

        [Fact]
        public void GetByExternalUuid_Returns_None()
        {
            //Arrange
            var inputId = A<Guid>();

            ExpectRepositoryContent(CreateSsoOrganizationIdentity(), CreateSsoOrganizationIdentity());

            //Act
            var result = _sut.GetByExternalUuid(inputId);

            //Assert
            Assert.False(result.HasValue);
        }

        private void ExpectRepositoryContent(params SsoOrganizationIdentity[] response)
        {
            _repository.Setup(x => x.AsQueryable()).Returns(response.AsQueryable());
        }

        private SsoOrganizationIdentity CreateSsoOrganizationIdentity(Guid? id = null)
        {
            return new SsoOrganizationIdentity
            {
                ExternalUuid = id.GetValueOrDefault(A<Guid>())
            };
        }
    }
}
