using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.LocalOptions.Base;
using Core.ApplicationServices.Model.LocalOptions;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Generic;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.LocalOptions
{
    public class TestLocalOptionEntity : LocalOptionEntity<TestOptionEntity> { }
    public class TestOptionEntity : OptionEntity<TestDomainModel> { }
    public class TestDomainModel { }
    public class GenericLocalOptionsServiceTest: WithAutoFixture
    {
        private Mock<IGenericRepository<TestOptionEntity>> _optionsRepository;
        private Mock<IGenericRepository<TestLocalOptionEntity>> _localOptionsRepository;
        private Mock<IAuthorizationContext> _authenticationContext;
        private Mock<IEntityIdentityResolver> _identityResolver;
        private Mock<IDomainEvents> _domainEvents;

        private GenericLocalOptionsService<TestLocalOptionEntity, TestDomainModel, TestOptionEntity> _sut;
        public GenericLocalOptionsServiceTest()
        {
            _optionsRepository = new Mock<IGenericRepository<TestOptionEntity>>();
            _localOptionsRepository = new Mock<IGenericRepository<TestLocalOptionEntity>>();
            _authenticationContext = new Mock<IAuthorizationContext>();
            _identityResolver = new Mock<IEntityIdentityResolver>();
            _domainEvents = new Mock<IDomainEvents>();

            _sut = new GenericLocalOptionsService<TestLocalOptionEntity, TestDomainModel, TestOptionEntity>(
                _optionsRepository.Object
                , _localOptionsRepository.Object,
                _authenticationContext.Object,
                _identityResolver.Object,
                _domainEvents.Object);
        }

        [Fact]
        public void Can_Get_Options_By_Organization_Uuid()
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var expectedLocal = SetupOptionRepositories(orgUuid);
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);

            var result = _sut.GetByOrganizationUuid(orgUuid);

            Assert.True(result.Ok);
            var options = result.Value;
            foreach (var option in options)
            {
                Assert.True(option.IsLocallyAvailable);
                var expectedDescription = expectedLocal.First(o => o.OptionId == option.Id).Description;
                Assert.Equal(expectedDescription, option.Description);
            }
        }

        [Fact]
        public void Can_Get_Option_By_Org_Uuid_And_Option_Id()
        {
            var orgUuid = A<Guid>();
            var optionId = A<int>();
            var expectedLocal = SetupOptionRepositories(orgUuid, optionId);
            var result = _sut.GetByOrganizationUuidAndOptionId(orgUuid, optionId);

            Assert.True(result.Ok);
            var option = result.Value;
            Assert.Equal(expectedLocal.First().Description, option.Description);
            Assert.Equal(optionId, option.Id);
            Assert.True(option.IsLocallyAvailable);
        }

        [Fact]
        public void Can_Create_Local_Option()
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var parameters = new LocalOptionCreateParameters()
            {
                OptionId = A<int>()
            };
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);
            _authenticationContext.Setup(_ => _.AllowCreate<TestLocalOptionEntity>(orgDbId)).Returns(true);

            var result = _sut.CreateLocalOption(orgUuid, parameters);

            ExpectCreateSuccess(result, parameters.OptionId, true);
        }

        [Fact]
        public void Create_Can_Activate_Existing_Local_Option()
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var optionId = A<int>();
            var parameters = new LocalOptionCreateParameters()
            {
                OptionId = optionId
            };
            SetupLocalRepositoryReturnsOneOption(optionId, orgUuid);
           _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);
            _authenticationContext.Setup(_ => _.AllowCreate<TestLocalOptionEntity>(orgDbId)).Returns(true);
            
            var result = _sut.CreateLocalOption(orgUuid, parameters);

            ExpectCreateSuccess(result, optionId);
        }

        [Fact]
        public void Create_Local_Option_Returns_Forbidden_If_Not_Authorized()
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var parameters = new LocalOptionCreateParameters()
            {
                OptionId = A<int>()
            };
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);
            _authenticationContext.Setup(_ => _.AllowCreate<TestLocalOptionEntity>(orgDbId)).Returns(false);

            var result = _sut.CreateLocalOption(orgUuid, parameters);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_Patch_Nonexisting_Local_Option_By_Creating_It()
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var optionId = A<int>();
            var parameters = new LocalOptionUpdateParameters()
            {
                Description = A<string>()
            };
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);
            _authenticationContext.Setup(_ => _.AllowCreate<TestLocalOptionEntity>(orgDbId)).Returns(true);


            var result = _sut.PatchLocalOption(orgUuid, optionId, parameters);

            Assert.True(result.Ok);
            Assert.Equal(parameters.Description, result.Value.Description);
            _localOptionsRepository.Verify(_ => _.Insert(It.IsAny<TestLocalOptionEntity>()));
            _localOptionsRepository.Verify(_ => _.Save());
        }

        [Fact]
        public void Can_Patch_Existing_Local_Option()
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var optionId = A<int>();
            var parameters = new LocalOptionUpdateParameters()
            {
                Description = A<string>()
            };
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);
            _authenticationContext.Setup(_ => _.AllowModify(It.IsAny<TestLocalOptionEntity>())).Returns(true);
            SetupLocalRepositoryReturnsOneOption(optionId, orgUuid);

            var result = _sut.PatchLocalOption(orgUuid, optionId, parameters);

            Assert.True(result.Ok);
            Assert.Equal(parameters.Description, result.Value.Description);
            _localOptionsRepository.Verify(_ => _.Update(It.IsAny<TestLocalOptionEntity>()));
            _localOptionsRepository.Verify(_ => _.Save());
        }

        private void SetupLocalRepositoryReturnsOneOption(int optionId, Guid orgUuid)
        {
            var existingOptionsList = new List<TestLocalOptionEntity>()
                { new() { OptionId = optionId, Organization = new Organization() { Uuid = orgUuid } } };
            _localOptionsRepository.Setup(_ => _.AsQueryable())
                .Returns(existingOptionsList.AsQueryable());
        }

        private void ExpectCreateSuccess(Result<TestLocalOptionEntity, OperationError> result, int optionId, bool expectNewlyInserted = false)
        {
            Assert.True(result.Ok);
            var option = result.Value;
            Assert.Equal(optionId, option.OptionId);
            Assert.True(option.IsActive);
            _localOptionsRepository.Verify(_ => _.Save(), Times.Once);
            var insertedTimes = expectNewlyInserted ? Times.Once() : Times.Never();
            _localOptionsRepository.Verify(_ => _.Insert(It.IsAny<TestLocalOptionEntity>()), insertedTimes);

        }

        private IList<TestLocalOptionEntity> SetupOptionRepositories(Guid orgUuid, int? staticOptionId = null)
        {
            var expectedGlobal = Enumerable.Range(1, 1)
                .Select(i => new TestOptionEntity()
                {
                    Id = staticOptionId ?? i,
                    IsEnabled = true,
                }).ToList();
            var expectedLocal = Enumerable.Range(1, 2)
                .Select(i => new TestLocalOptionEntity()
                {
                    OptionId = staticOptionId ?? i,
                    Description = A<string>(),
                    IsActive = true,
                    Organization = new Organization() { Uuid = orgUuid }
                }).ToList();
            _optionsRepository.Setup(_ => _.AsQueryable()).Returns(expectedGlobal.AsQueryable());
            _localOptionsRepository.Setup(_ => _.AsQueryable()).Returns(expectedLocal.AsQueryable());

            return expectedLocal;
        }
    }
}
