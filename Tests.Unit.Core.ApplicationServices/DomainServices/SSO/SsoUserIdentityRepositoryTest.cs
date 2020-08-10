using System;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Result;
using Core.DomainModel.SSO;
using Core.DomainServices;
using Core.DomainServices.Repositories.SSO;
using Moq;
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

        [Fact]
        public void AddNew_Returns_New_Identity()
        {
            //Arrange
            var externalId = A<Guid>();
            var user = new User();
            ExpectRepositoryContent(CreateSsoUserIdentity(), CreateSsoUserIdentity());
            _repository.Setup(x => x.Insert(It.IsAny<SsoUserIdentity>())).Returns((SsoUserIdentity input) => input);

            //Act
            var identityResult = _sut.AddNew(user, externalId);

            //Assert
            Assert.True(identityResult.Ok);
            var identity = identityResult.Value;
            Assert.NotNull(identityResult);
            Assert.Same(user, identity.User);
            Assert.Equal(externalId, identity.ExternalUuid);
        }

        [Fact]
        public void AddNew_Returns_Conflict_If_Existing_Record_Exists()
        {
            //Arrange
            var externalId = A<Guid>();
            var user = new User();
            ExpectRepositoryContent(CreateSsoUserIdentity(), CreateSsoUserIdentity(externalId));

            //Act
            var identityResult = _sut.AddNew(user, externalId);

            //Assert
            Assert.False(identityResult.Ok);
            Assert.Equal(OperationFailure.Conflict, identityResult.Error.FailureType);
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
