using System;
using System.Linq;
using Core.DomainModel.Result;
using Core.DomainModel.SSO;
using Core.DomainServices;
using Core.DomainServices.Repositories.SSO;
using Moq;
using Moq.Language.Flow;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.SSO
{
    public class SsoUserIdentityRepositoryTest : WithAutoFixture
    {
        private readonly Mock<IGenericRepository<SsoUserIdentity>> _repository;
        private readonly SsoUserIdentityRepository _sut;

        public SsoUserIdentityRepositoryTest()
        {
            _repository = new Mock<IGenericRepository<SsoUserIdentity>>();
            _sut = new SsoUserIdentityRepository(_repository.Object);
        }

        [Fact]
        public void GetByExternalUuid_Returns_Value()
        {
            //Arrange
            var inputId = A<Guid>();
            var expectedResponse = CreateSsoUserIdentity(inputId);

            ExpectRepositoryContent(CreateSsoUserIdentity(), expectedResponse);

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

            ExpectRepositoryContent(CreateSsoUserIdentity(), CreateSsoUserIdentity());

            //Act
            var result = _sut.GetByExternalUuid(inputId);

            //Assert
            Assert.False(result.HasValue);
        }

        private void ExpectRepositoryContent(params SsoUserIdentity[] response)
        {
            _repository.Setup(x => x.AsQueryable()).Returns(response.AsQueryable());
        }

        private SsoUserIdentity CreateSsoUserIdentity(Guid? id = null)
        {
            return new SsoUserIdentity
            {
                ExternalUuid = id.GetValueOrDefault(A<Guid>())
            };
        }
    }
}
