using System;
using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
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

        [Fact]
        public void AddNew_Returns_New_Identity()
        {
            //Arrange
            var externalId = A<Guid>();
            var organization = new Organization();
            ExpectRepositoryContent(CreateSsoOrganizationIdentity(), CreateSsoOrganizationIdentity());
            _repository.Setup(x => x.Insert(It.IsAny<SsoOrganizationIdentity>())).Returns((SsoOrganizationIdentity input) => input);

            //Act
            var identityResult = _sut.AddNew(organization, externalId);

            //Assert
            Assert.True(identityResult.Ok);
            var identity = identityResult.Value;
            Assert.NotNull(identityResult);
            Assert.Same(organization, identity.Organization);
            Assert.Equal(externalId, identity.ExternalUuid);
        }

        [Fact]
        public void AddNew_Returns_Conflict_If_Existing_Record_Exists()
        {
            //Arrange
            var externalId = A<Guid>();
            var organization = new Organization();
            ExpectRepositoryContent(CreateSsoOrganizationIdentity(externalId), CreateSsoOrganizationIdentity());

            //Act
            var identityResult = _sut.AddNew(organization, externalId);

            //Assert
            Assert.False(identityResult.Ok);
            Assert.Equal(OperationFailure.Conflict, identityResult.Error.FailureType);
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
