using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.LocalOptions;
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
    public class TestOptionEntity : OptionEntity<TestReferenceType> { }
    public class TestReferenceType { }
    public class GenericLocalOptionsServiceTest: WithAutoFixture
    {
        private Mock<IGenericRepository<TestOptionEntity>> _optionsRepository;
        private Mock<IGenericRepository<TestLocalOptionEntity>> _localOptionsRepository;
        private Mock<IAuthorizationContext> _authorizationContext;
        private Mock<IEntityIdentityResolver> _identityResolver;
        private Mock<IDomainEvents> _domainEvents;

        private BaseGenericLocalOptionsService<TestLocalOptionEntity, TestReferenceType, TestOptionEntity> _sut;
        public GenericLocalOptionsServiceTest()
        {
            _optionsRepository = new Mock<IGenericRepository<TestOptionEntity>>();
            _localOptionsRepository = new Mock<IGenericRepository<TestLocalOptionEntity>>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _identityResolver = new Mock<IEntityIdentityResolver>();
            _domainEvents = new Mock<IDomainEvents>();

            _sut = new BaseGenericLocalOptionsService<TestLocalOptionEntity, TestReferenceType, TestOptionEntity>(
                _optionsRepository.Object
                , _localOptionsRepository.Object,
                _authorizationContext.Object,
                _identityResolver.Object,
                _domainEvents.Object);
        }

        private static void UpdateLocalRoleOptionValues(TestOptionEntity option, TestLocalOptionEntity localOption)
        {
            option.UpdateLocalOptionValues(localOption);
        }


        [Fact]
        public void Get_By_Org_Uuid_Returns_Global_Options_If_No_Local_Changes()
        {
            var orgUuid = A<Guid>();
            var expectedGlobal = Enumerable.Range(1, 1)
                .Select(i => new TestOptionEntity()
                {
                    Id = i,
                    IsEnabled = true,
                }).ToList();
            var expectedLocal = new List<TestLocalOptionEntity>();
            _optionsRepository.Setup(_ => _.AsQueryable()).Returns(expectedGlobal.AsQueryable());
            _localOptionsRepository.Setup(_ => _.AsQueryable()).Returns(expectedLocal.AsQueryable());

            var options = _sut.BaseGetLocalOptions(orgUuid, UpdateLocalRoleOptionValues);

            foreach (var option in options)
            {
                var expectedName = expectedGlobal.First(o => o.Id == option.Id).Name;
                Assert.Equal(expectedName, option.Name);
                var expectedDescription = expectedGlobal.First(o => o.Id == option.Id).Description;
                Assert.Equal(expectedDescription, option.Description);
            }
        }

        [Fact]
        public void Can_Get_Options_By_Organization_Uuid()
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var expectedLocal = SetupOptionRepositories(orgUuid);
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);

            var options = _sut.BaseGetLocalOptions(orgUuid, UpdateLocalRoleOptionValues);

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
            var globalOptionUuid = A<Guid>();
            var optionId = A<int>();
            SetupResolveGlobalOptionUuid(globalOptionUuid, optionId);
            var expectedLocal = SetupOptionRepositories(orgUuid, optionId);
            var result = _sut.BaseGetLocalOption(orgUuid, globalOptionUuid, UpdateLocalRoleOptionValues);
            
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
            var optionUuid = A<Guid>();
            var optionId = A<int>();
            var parameters = new LocalOptionCreateParameters()
            {
                OptionUuid = optionUuid
            };
            SetupLocalOptionsRepositoryReturnsNoneThenOne(optionId, orgUuid, A<string>());
            SetupCommonMocksForSuccessfulCreate(orgUuid, orgDbId, optionId, optionUuid);
            SetupResolveGlobalOptionUuid(optionUuid, optionId);

            var result = _sut.BaseCreateLocalOption(orgUuid, parameters, UpdateLocalRoleOptionValues);

            ExpectCreateSuccess(result, parameters.OptionUuid, true);
        }

        [Fact]
        public void Create_Can_Activate_Existing_Local_Option()
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var optionId = A<int>();
            var optionUuid = A<Guid>();
            var parameters = new LocalOptionCreateParameters()
            {
                OptionUuid = optionUuid
            };
            var existingLocalOptionsList = new List<TestLocalOptionEntity>()
                { new() { OptionId = optionId, Organization = new Organization() { Uuid = orgUuid }, IsActive = false} };
            _localOptionsRepository.SetupSequence(_ => _.AsQueryable())
                .Returns(existingLocalOptionsList.AsQueryable())
                .Returns(existingLocalOptionsList.AsQueryable());
            SetupCommonMocksForSuccessfulCreate(orgUuid, orgDbId, optionId, optionUuid);
            SetupResolveGlobalOptionUuid(optionUuid, optionId);

            var result = _sut.BaseCreateLocalOption(orgUuid, parameters, UpdateLocalRoleOptionValues);

            ExpectCreateSuccess(result, optionUuid);
        }

        private void SetupCommonMocksForSuccessfulCreate(Guid orgUuid, int orgDbId, int optionId, Guid optionUuid)
        {
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);
            _authorizationContext.Setup(_ => _.AllowCreate<TestLocalOptionEntity>(orgDbId)).Returns(true);
            SetupGlobalOptionsRepositoryReturnsOneOption(optionId, optionUuid);
        }

        [Fact]
        public void Create_Local_Option_Returns_Forbidden_If_Not_Authorized()
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var parameters = new LocalOptionCreateParameters()
            {
                OptionUuid = A<Guid>()
            };
            var optionId = A<int>();
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);
            SetupResolveAnyGlobalOptionUuid(optionId);
            _authorizationContext.Setup(_ => _.AllowCreate<TestLocalOptionEntity>(orgDbId)).Returns(false);

            var result = _sut.BaseCreateLocalOption(orgUuid, parameters, UpdateLocalRoleOptionValues);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_Patch_Nonexisting_Local_Option_By_Creating_It()
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var optionId = A<int>();
            var optionUuid = A<Guid>();
            var parameters = new LocalOptionUpdateParameters()
            {
                Description = Maybe<string>.Some(A<string>()).AsChangedValue()
            };
            var descriptionText = parameters.Description.NewValue.Value;
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);
            _authorizationContext.Setup(_ => _.AllowCreate<TestLocalOptionEntity>(orgDbId)).Returns(true);
            SetupLocalOptionsRepositoryReturnsNoneThenOne(optionId, orgUuid, descriptionText);
            SetupGlobalOptionsRepositoryReturnsOneOption(optionId, optionUuid);
            SetupResolveGlobalOptionUuid(optionUuid, optionId);

            var result = _sut.BasePatchLocalOption(orgUuid, optionUuid, parameters, UpdateLocalRoleOptionValues);

            Assert.True(result.Ok);
            Assert.Equal(descriptionText, result.Value.Description);
            _localOptionsRepository.Verify(_ => _.Insert(It.IsAny<TestLocalOptionEntity>()));
            _localOptionsRepository.Verify(_ => _.Save());
        }

        private void SetupLocalOptionsRepositoryReturnsNoneThenOne(int optionId, Guid orgUuid, string description)
        {
            var existingLocalOptionsList = new List<TestLocalOptionEntity>()
                { new() { OptionId = optionId, Organization = new Organization() { Uuid = orgUuid }, IsActive = true, Description = description} };
            _localOptionsRepository.SetupSequence(_ => _.AsQueryable())
                .Returns(new List<TestLocalOptionEntity>().AsQueryable())
                .Returns(existingLocalOptionsList.AsQueryable());
        }

        [Fact]
        public void Can_Patch_Existing_Local_Option()
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var optionId = A<int>();
            var optionUuid = A<Guid>();
            var parameters = new LocalOptionUpdateParameters()
            {
                Description = Maybe<string>.Some(A<string>()).AsChangedValue(),
            };
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);
            _authorizationContext.Setup(_ => _.AllowModify(It.IsAny<TestLocalOptionEntity>())).Returns(true);
            SetupLocalOptionsRepositoryReturnsOneOptionTwice(optionId, orgUuid);
            SetupGlobalOptionsRepositoryReturnsOneOption(optionId, optionUuid);
            SetupResolveGlobalOptionUuid(optionUuid, optionId);

            var result = _sut.BasePatchLocalOption(orgUuid, optionUuid, parameters, UpdateLocalRoleOptionValues);

            Assert.True(result.Ok);
            Assert.Equal(parameters.Description.NewValue.Value, result.Value.Description);
            _localOptionsRepository.Verify(_ => _.Update(It.IsAny<TestLocalOptionEntity>()));
            _localOptionsRepository.Verify(_ => _.Save());
        }

        private void SetupGlobalOptionsRepositoryReturnsOneOption(int optionId, Guid optionUuid)
        {
            var existingGlobalOptions = new List<TestOptionEntity>()
            {
                new ()
                {
                    Id = optionId,
                    IsEnabled = true,
                    Uuid = optionUuid
                }
            };
            _optionsRepository.Setup(_ => _.AsQueryable()).Returns(existingGlobalOptions.AsQueryable());
        }

        private void SetupLocalOptionsRepositoryReturnsOneOptionTwice(int optionId, Guid orgUuid)
        {
            var existingLocalOptionsList = new List<TestLocalOptionEntity>()
                { new() { OptionId = optionId, Organization = new Organization() { Uuid = orgUuid }, IsActive = false} };
            _localOptionsRepository.SetupSequence(_ => _.AsQueryable())
                .Returns(existingLocalOptionsList.AsQueryable())
                .Returns(existingLocalOptionsList.AsQueryable());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Patch_Returns_Forbidden_If_Unauthorized(bool optionExists)
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var optionId = A<int>();
            var optionUuid = A<Guid>();
            var parameters = new LocalOptionUpdateParameters()
            {
                Description = Maybe<string>.Some(A<string>()).AsChangedValue()
            };
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);
            if (optionExists)
            {
                SetupLocalRepositoryReturnsOneOption(optionId, orgUuid);
                _authorizationContext.Setup(_ => _.AllowModify(It.IsAny<TestLocalOptionEntity>())).Returns(false);
            }
            else
            {
                _authorizationContext.Setup(_ => _.AllowCreate<TestLocalOptionEntity>(orgDbId)).Returns(false);
            }
            SetupResolveGlobalOptionUuid(optionUuid, optionId);


            var result = _sut.BasePatchLocalOption(orgUuid, optionUuid, parameters, UpdateLocalRoleOptionValues);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Delete_Can_Deactivate_Local_Option()
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var optionId = A<int>();
            var optionUuid = A<Guid>();
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);
            _authorizationContext.Setup(_ => _.AllowDelete(It.IsAny<TestLocalOptionEntity>())).Returns(true);
            SetupLocalOptionsRepositoryReturnsOneOptionTwice(optionId, orgUuid);
            SetupGlobalOptionsRepositoryReturnsOneOption(optionId, optionUuid);
            SetupResolveGlobalOptionUuid(optionUuid, optionId);

            var result = _sut.BaseDeleteLocalOption(orgUuid, optionUuid, UpdateLocalRoleOptionValues);

            Assert.True(result.Ok);
            Assert.False(result.Value.IsLocallyAvailable);
            _localOptionsRepository.Verify(_ => _.Save());
        }

        [Fact]
        public void Delete_Returns_Forbidden_If_Unauthorized()
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var optionId = A<int>();
            var optionUuid = A<Guid>();
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);
            _authorizationContext.Setup(_ => _.AllowDelete(It.IsAny<TestLocalOptionEntity>())).Returns(false);
            SetupLocalRepositoryReturnsOneOption(optionId, orgUuid);
            SetupResolveGlobalOptionUuid(optionUuid, optionId);

            var result = _sut.BaseDeleteLocalOption(orgUuid, optionUuid, UpdateLocalRoleOptionValues);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Delete_Creates_Before_Deactivating_If_No_Local_Option_Found()
        {
            var orgUuid = A<Guid>();
            var orgDbId = A<int>();
            var optionId = A<int>();
            var optionUuid = A<Guid>();
            _identityResolver.SetupSequence(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(orgDbId);
            _identityResolver.SetupSequence(_ => _.ResolveDbId<TestOptionEntity>(optionUuid))
                .Returns(optionId)
                .Returns(optionId)
                .Returns(optionId);
            _authorizationContext.Setup(_ => _.AllowDelete(It.IsAny<TestLocalOptionEntity>())).Returns(true);
            _authorizationContext.Setup(_ => _.AllowCreate<TestLocalOptionEntity>(orgDbId)).Returns(true);
            var createdLocalOptionList = new List<TestLocalOptionEntity>()
            {
                new ()
                {
                    OptionId = optionId, 
                    Organization = new Organization(){ Uuid = orgUuid }
                }
            };
            _localOptionsRepository.SetupSequence(_ => _.AsQueryable())
                .Returns(new List<TestLocalOptionEntity>().AsQueryable())
                .Returns(new List<TestLocalOptionEntity>().AsQueryable())
                .Returns(createdLocalOptionList.AsQueryable())
                .Returns(createdLocalOptionList.AsQueryable())
                .Returns(createdLocalOptionList.AsQueryable());
            SetupResolveGlobalOptionUuid(optionUuid, optionId);
            var expectedGlobal = Enumerable.Range(1, 1)
                .Select(i => new TestOptionEntity()
                {
                    Id = optionId,
                    IsEnabled = true,
                    Uuid = optionUuid
                }).ToList();
            _optionsRepository.Setup(_ => _.AsQueryable()).Returns(expectedGlobal.AsQueryable());


            var result = _sut.BaseDeleteLocalOption(orgUuid, optionUuid, UpdateLocalRoleOptionValues);

            Assert.True(result.Ok);
            Assert.False(result.Value.IsLocallyAvailable);
            _localOptionsRepository.Verify(_ => _.Insert(It.IsAny<TestLocalOptionEntity>()));
        }

        private void SetupLocalRepositoryReturnsOneOption(int optionId, Guid orgUuid)
        {
            var existingOptionsList = new List<TestLocalOptionEntity>()
                { new() { OptionId = optionId, Organization = new Organization() { Uuid = orgUuid } } };
            _localOptionsRepository.Setup(_ => _.AsQueryable())
                .Returns(existingOptionsList.AsQueryable());
        }

        private void SetupResolveGlobalOptionUuid(Guid uuid, int id)
        {
            _identityResolver.Setup(_ => _.ResolveDbId<TestOptionEntity>(uuid)).Returns(id);
        }

        private void SetupResolveAnyGlobalOptionUuid(int id)
        {
            _identityResolver.Setup(_ => _.ResolveDbId<TestOptionEntity>(It.IsAny<Guid>())).Returns(id);
        }

        private void ExpectCreateSuccess(Result<TestOptionEntity, OperationError> result, Guid optionUuid, bool expectNewlyInserted = false)
        {
            Assert.True(result.Ok);
            var option = result.Value;
            Assert.Equal(optionUuid, option.Uuid);
            Assert.True(option.IsLocallyAvailable);
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
