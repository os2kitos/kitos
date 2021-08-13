using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.References;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.SystemUsage
{
    public class ItSystemUsageServiceTest : WithAutoFixture
    {
        private readonly ItSystemUsageService _sut;
        private readonly Mock<IGenericRepository<ItSystemUsage>> _usageRepository;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IItSystemRepository> _systemRepository;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IDomainEvents> _domainEvents;
        private readonly Mock<IReferenceService> _referenceService;
        private readonly Mock<IGenericRepository<ItSystemUsageSensitiveDataLevel>> _sensitiveDataLevelRepository;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;

        public ItSystemUsageServiceTest()
        {
            _usageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _systemRepository = new Mock<IItSystemRepository>();
            _transactionManager = new Mock<ITransactionManager>();
            _domainEvents = new Mock<IDomainEvents>();
            _referenceService = new Mock<IReferenceService>();
            _sensitiveDataLevelRepository = new Mock<IGenericRepository<ItSystemUsageSensitiveDataLevel>>();
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _sut = new ItSystemUsageService(
                _usageRepository.Object,
                _authorizationContext.Object,
                _systemRepository.Object,
                _referenceService.Object,
                _transactionManager.Object,
                _domainEvents.Object,
                _sensitiveDataLevelRepository.Object,
                _userContextMock.Object,
                new Mock<IAttachedOptionRepository>().Object);
        }

        [Fact]
        public void Query_Returns_All_If_User_Has_Full_CrossOrgAccess()
        {
            //Arrange
            var itSystemUsage1 = CreateSystemUsage(A<int>(), CreateItSystem());
            var itSystemUsage2 = CreateSystemUsage(A<int>(), CreateItSystem());
            var itSystemUsage3 = CreateSystemUsage(A<int>(), CreateItSystem());
            ExpectUsageRepositoryAsQueryable(itSystemUsage1, itSystemUsage2, itSystemUsage3);
            ExpectGetCrossOrganizationReadAccessReturns(CrossOrganizationDataReadAccessLevel.All);

            //Act
            var result = _sut.Query();

            //Assert
            Assert.Equal(new[]{itSystemUsage1,itSystemUsage2,itSystemUsage3},result.ToList());
        }

        [Theory]
        [InlineData(CrossOrganizationDataReadAccessLevel.None)]
        [InlineData(CrossOrganizationDataReadAccessLevel.Public)]
        [InlineData(CrossOrganizationDataReadAccessLevel.RightsHolder)]
        public void Query_Returns_All_From_Own_Organizations_If_User_Has_Less_Than_Full_CrossOrgAccess(CrossOrganizationDataReadAccessLevel accessLevel)
        {
            //Arrange
            var itSystemUsage1 = CreateSystemUsage(A<int>(), CreateItSystem());
            var itSystemUsage2 = CreateSystemUsage(A<int>(), CreateItSystem());
            var itSystemUsage3 = CreateSystemUsage(A<int>(), CreateItSystem());
            ExpectUsageRepositoryAsQueryable(itSystemUsage1, itSystemUsage2, itSystemUsage3);
            ExpectGetCrossOrganizationReadAccessReturns(accessLevel);
            _userContextMock.Setup(x=>x.OrganizationIds).Returns(new []{itSystemUsage1.OrganizationId, itSystemUsage3.OrganizationId});

            //Act
            var result = _sut.Query();

            //Assert that only usages from orgs with membership are included
            Assert.Equal(new[] { itSystemUsage1, itSystemUsage3 }, result.ToList());
        }

        [Fact]
        public void Query_Applies_SubQueries()
        {
            //Arrange
            var itSystemUsage1 = CreateSystemUsage(A<int>(), CreateItSystem());
            var itSystemUsage2 = CreateSystemUsage(A<int>(), CreateItSystem());
            var itSystemUsage3 = CreateSystemUsage(A<int>(), CreateItSystem());
            ExpectUsageRepositoryAsQueryable(itSystemUsage1, itSystemUsage2, itSystemUsage3);
            ExpectGetCrossOrganizationReadAccessReturns(CrossOrganizationDataReadAccessLevel.All);
            var subQuery1 = new Mock<IDomainQuery<ItSystemUsage>>();
            var subQuery2 = new Mock<IDomainQuery<ItSystemUsage>>();
            subQuery1.Setup(x => x.Apply(It.IsAny<IQueryable<ItSystemUsage>>())).Returns<IQueryable<ItSystemUsage>>(input => input.Skip(1));
            subQuery2.Setup(x => x.Apply(It.IsAny<IQueryable<ItSystemUsage>>())).Returns<IQueryable<ItSystemUsage>>(input => input.Skip(1));

            //Act
            var result = _sut.Query(subQuery1.Object,subQuery2.Object);

            //Assert
            Assert.Equal(new[] { itSystemUsage3 }, result.ToList());
        }

        [Fact]
        public void GetByUuid_Returns_ItSystemUsage()
        {
            //Arrange
            var uuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage { Uuid = uuid };
            ExpectUsageRepositoryAsQueryable(itSystemUsage);
            ExpectAllowReadReturns(itSystemUsage, true);

            //Act
            var result = _sut.GetByUuid(uuid);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itSystemUsage, result.Value);
        }

        [Fact]
        public void GetByUuid_Returns_NotFound()
        {
            //Arrange
            var uuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage { Uuid = A<Guid>() };
            ExpectUsageRepositoryAsQueryable(itSystemUsage);

            //Act
            var result = _sut.GetByUuid(uuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void GetByUuid_Returns_Forbidden()
        {
            //Arrange
            var uuid = A<Guid>();
            var itSystemUsage = new ItSystemUsage { Uuid = uuid };
            ExpectUsageRepositoryAsQueryable(itSystemUsage);
            ExpectAllowReadReturns(itSystemUsage, false);

            //Act
            var result = _sut.GetByUuid(uuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Add_Returns_Conflict_If_System_Already_In_Use()
        {
            //Arrange
            var organizationId = A<int>();
            var systemId = A<int>();
            var systemUsage = SetupRepositoryQueryWith(organizationId, systemId);

            //Act
            var result = _sut.Add(systemUsage);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Conflict, result.Error.FailureType);
        }

        [Fact]
        public void Add_Returns_Forbidden_Not_Allowed_To_Create()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(itSystemUsage.OrganizationId, itSystemUsage)).Returns(false);

            //Act
            var result = _sut.Add(itSystemUsage);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Add_Returns_BadInput_If_ItSystemNotFound()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage { ItSystemId = A<int>() };
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(itSystemUsage.OrganizationId, itSystemUsage)).Returns(true);
            _systemRepository.Setup(x => x.GetSystem(itSystemUsage.ItSystemId)).Returns(default(ItSystem));

            //Act
            var result = _sut.Add(itSystemUsage);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Add_Returns_Forbidden_If_ReadAccess_To_ItSystem_Is_Declined()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage { ItSystemId = A<int>(), OrganizationId = A<int>() };
            var itSystem = new ItSystem();
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(itSystemUsage.OrganizationId, itSystemUsage)).Returns(true);
            _systemRepository.Setup(x => x.GetSystem(itSystemUsage.ItSystemId)).Returns(itSystem);
            _authorizationContext.Setup(x => x.AllowReads(itSystem)).Returns(false);

            //Act
            var result = _sut.Add(itSystemUsage);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Add_Returns_Forbidden_If_Usage_Of_Local_System_In_Other_Org_Is_Attempted()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage { ItSystemId = A<int>(), OrganizationId = A<int>() };
            var itSystem = new ItSystem() { OrganizationId = itSystemUsage.OrganizationId + 1, AccessModifier = AccessModifier.Local };
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(itSystemUsage.OrganizationId, itSystemUsage)).Returns(true);
            _systemRepository.Setup(x => x.GetSystem(itSystemUsage.ItSystemId)).Returns(itSystem);
            _authorizationContext.Setup(x => x.AllowReads(itSystem)).Returns(true);

            //Act
            var result = _sut.Add(itSystemUsage);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Add_Returns_Ok(bool sameOrg)
        {
            //Arrange
            var input = new ItSystemUsage
            {
                ItSystemId = A<int>(),
                OrganizationId = A<int>()
            };
            var associatedItSystem = new ItSystem
            {
                OrganizationId = sameOrg ? input.OrganizationId : input.OrganizationId + 1,
                AccessModifier = AccessModifier.Public
            };
            var usageCreatedByRepo = new ItSystemUsage();
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(input.OrganizationId, input)).Returns(true);
            _systemRepository.Setup(x => x.GetSystem(input.ItSystemId)).Returns(associatedItSystem);
            _authorizationContext.Setup(x => x.AllowReads(associatedItSystem)).Returns(true);
            _usageRepository.Setup(x => x.Create()).Returns(usageCreatedByRepo);

            //Act
            var result = _sut.Add(input);

            //Assert
            Assert.True(result.Ok);
            var createdUsage = result.Value;
            Assert.NotSame(input, createdUsage);
            Assert.Same(usageCreatedByRepo, createdUsage);
            Assert.Equal(input.OrganizationId, createdUsage.OrganizationId);
            Assert.Equal(input.ItSystemId, createdUsage.ItSystemId);
            _usageRepository.Verify(x => x.Insert(usageCreatedByRepo), Times.Once);
            _usageRepository.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void Delete_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            ExpectGetUsageByKeyReturns(id, null);

            //Act
            var result = _sut.Delete(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void Delete_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var itSystemUsage = new ItSystemUsage();
            ExpectGetUsageByKeyReturns(id, itSystemUsage);
            _authorizationContext.Setup(x => x.AllowDelete(itSystemUsage)).Returns(false);

            //Act
            var result = _sut.Delete(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void Delete_Returns_Ok()
        {
            //Arrange
            var id = A<int>();
            var itSystemUsage = new ItSystemUsage();
            var transaction = new Mock<IDatabaseTransaction>();
            ExpectGetUsageByKeyReturns(id, itSystemUsage);
            _authorizationContext.Setup(x => x.AllowDelete(itSystemUsage)).Returns(true);
            _transactionManager.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);
            _referenceService.Setup(x => x.DeleteBySystemUsageId(id)).Returns(Result<IEnumerable<ExternalReference>, OperationFailure>.Success(new ExternalReference[0]));

            //Act
            var result = _sut.Delete(id);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(itSystemUsage, result.Value);
            _usageRepository.Verify(x => x.DeleteByKeyWithReferencePreload(id), Times.Once);
            _usageRepository.Verify(x => x.Save(), Times.Once);
            _referenceService.Verify(x => x.DeleteBySystemUsageId(id), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
            _domainEvents.Verify(x => x.Raise(It.Is<EntityLifeCycleEvent<ItSystemUsage>>(ev => ev.Entity == itSystemUsage && ev.ChangeType == LifeCycleEventType.Deleted)));
        }

        [Fact]
        public void GetByOrganizationAndSystemId_Returns_Nothing()
        {
            //Arrange
            var systemId = A<int>();
            var organizationId = A<int>();
            SetupRepositoryQueryWith(organizationId + 1, systemId + 1);

            //Act
            var result = _sut.GetByOrganizationAndSystemId(organizationId, systemId);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetByOrganizationAndSystemId_Returns()
        {
            //Arrange
            var systemId = A<int>();
            var organizationId = A<int>();
            var systemUsage = SetupRepositoryQueryWith(organizationId, systemId);

            //Act
            var result = _sut.GetByOrganizationAndSystemId(organizationId, systemId);

            //Assert
            Assert.Same(systemUsage, result);
        }

        [Fact]
        public void GetById_Returns()
        {
            //Arrange
            var id = A<int>();
            var itSystemUsage = new ItSystemUsage();
            ExpectGetUsageByKeyReturns(id, itSystemUsage);

            //Act
            var result = _sut.GetById(id);

            //Assert
            Assert.Same(itSystemUsage, result);
        }

       

        [Theory]
        [InlineData(SensitiveDataLevel.NONE)]
        [InlineData(SensitiveDataLevel.PERSONALDATA)]
        [InlineData(SensitiveDataLevel.SENSITIVEDATA)]
        [InlineData(SensitiveDataLevel.LEGALDATA)]
        public void AddSensitiveData_Returns_Ok(SensitiveDataLevel sensitiveDataLevel)
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);
            ExpectAllowModifyReturns(itSystemUsage, true);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);

            //Act
            var result = _sut.AddSensitiveDataLevel(itSystemUsage.Id, sensitiveDataLevel);

            //Assert
            Assert.True(result.Ok);
            var addedSensitiveData = result.Value;
            Assert.Equal(sensitiveDataLevel, addedSensitiveData.SensitivityDataLevel);
        }

        [Fact]
        public void AddSensitiveData_Returns_NotFound_If_No_System_Usage()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);

            //Act
            var result = _sut.AddSensitiveDataLevel(itSystemUsage.Id, A<SensitiveDataLevel>());

            //Assert
            AssertSensitiveDataLevelError(result, OperationFailure.NotFound);
        }

        [Fact]
        public void AddSensitiveData_Returns_Forbidden_If_User_Not_Allowed_To_Modify()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);
            ExpectAllowModifyReturns(itSystemUsage, false);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);

            //Act
            var result = _sut.AddSensitiveDataLevel(itSystemUsage.Id, A<SensitiveDataLevel>());

            //Assert
            AssertSensitiveDataLevelError(result, OperationFailure.Forbidden);
        }

        [Theory]
        [InlineData(SensitiveDataLevel.NONE)]
        [InlineData(SensitiveDataLevel.PERSONALDATA)]
        [InlineData(SensitiveDataLevel.SENSITIVEDATA)]
        [InlineData(SensitiveDataLevel.LEGALDATA)]
        public void RemoveSensitiveData_Returns_Ok(SensitiveDataLevel sensitiveDataLevel)
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);
            itSystemUsage.SensitiveDataLevels.Add(new ItSystemUsageSensitiveDataLevel()
            {
                ItSystemUsage = itSystemUsage,
                SensitivityDataLevel = sensitiveDataLevel
            });
            ExpectAllowModifyReturns(itSystemUsage, true);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);

            //Act
            var result = _sut.RemoveSensitiveDataLevel(itSystemUsage.Id, sensitiveDataLevel);

            //Assert
            Assert.True(result.Ok);
            var removedSensitiveData = result.Value;
            Assert.Equal(sensitiveDataLevel, removedSensitiveData.SensitivityDataLevel);
        }

        [Fact]
        public void RemoveSensitiveData_Returns_NotFound_If_No_System_Usage()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);

            //Act
            var result = _sut.RemoveSensitiveDataLevel(itSystemUsage.Id, A<SensitiveDataLevel>());

            //Assert
            AssertSensitiveDataLevelError(result, OperationFailure.NotFound);
        }

        [Fact]
        public void RemoveSensitiveData_Returns_BadInput_If_DataLevel_Not_On_System_Usage()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);
            ExpectAllowModifyReturns(itSystemUsage, true);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);

            //Act
            var result = _sut.RemoveSensitiveDataLevel(itSystemUsage.Id, A<SensitiveDataLevel>());

            //Assert
            AssertSensitiveDataLevelError(result, OperationFailure.BadInput);
        }

        [Fact]
        public void RemoveSensitiveData_Returns_Forbidden_If_User_Not_Allowed_To_Modify()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);
            ExpectAllowModifyReturns(itSystemUsage, false);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);

            //Act
            var result = _sut.RemoveSensitiveDataLevel(itSystemUsage.Id, A<SensitiveDataLevel>());

            //Assert
            AssertSensitiveDataLevelError(result, OperationFailure.Forbidden);
        }

        private void AssertSensitiveDataLevelError(
            Result<ItSystemUsageSensitiveDataLevel, OperationError> sensitiveDataLevelResult, OperationFailure failure)
        {
            Assert.False(sensitiveDataLevelResult.Ok);
            var operationError = sensitiveDataLevelResult.Error;
            Assert.Equal(failure, operationError.FailureType);
        }

      
        private void ExpectGetCrossOrganizationReadAccessReturns(CrossOrganizationDataReadAccessLevel accessLevel)
        {
            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(accessLevel);
        }

        private ItSystemUsage CreateSystemUsage(int organizationId, ItSystem itSystem)
        {
            return new ItSystemUsage
            {
                OrganizationId = organizationId,
                Id = A<int>(),
                ItSystemId = itSystem.Id,
                ItSystem = itSystem
            };
        }

        private ItSystem CreateItSystem()
        {
            return new ItSystem
            {
                Name = A<string>(),
                Id = A<int>()
            };
        }


        private ItSystemUsage SetupRepositoryQueryWith(int organizationId, int systemId)
        {
            var itSystemUsage = new ItSystemUsage { OrganizationId = organizationId, ItSystemId = systemId };

            _usageRepository.Setup(x => x.AsQueryable())
                .Returns(new[] { itSystemUsage }.AsQueryable());

            return itSystemUsage;
        }

        private void SetupRepositoryQueryWith(IEnumerable<ItSystemUsage> response = null)
        {
            response = response ?? new ItSystemUsage[0];
            _usageRepository.Setup(x => x.AsQueryable()).Returns(response.AsQueryable());
        }

        private void ExpectGetUsageByKeyReturns(int id, ItSystemUsage itSystemUsage)
        {
            _usageRepository.Setup(x => x.GetByKey(id)).Returns(itSystemUsage);
        }

        private void ExpectAllowModifyReturns(ItSystemUsage source, bool value)
        {
            _authorizationContext.Setup(x => x.AllowModify(source)).Returns(value);
        }

        private void ExpectAllowReadReturns(IEntity entity, bool value)
        {
            _authorizationContext.Setup(x => x.AllowReads(entity)).Returns(value);
        }

        private void ExpectUsageRepositoryAsQueryable(params ItSystemUsage[] itSystemUsages)
        {
            _usageRepository.Setup(x => x.AsQueryable()).Returns(itSystemUsages.AsQueryable());
        }
    }
}
