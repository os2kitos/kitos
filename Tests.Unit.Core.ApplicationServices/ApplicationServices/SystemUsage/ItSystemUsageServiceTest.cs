using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.References;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;
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
        private readonly Mock<IGenericRepository<ArchivePeriod>> _archivePeriodRepositoryMock;
        private readonly Mock<IGenericRepository<ItSystemUsagePersonalData>> _personalDataOptionsRepository;

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
            _archivePeriodRepositoryMock = new Mock<IGenericRepository<ArchivePeriod>>();
            _personalDataOptionsRepository = new Mock<IGenericRepository<ItSystemUsagePersonalData>>();
            _sut = new ItSystemUsageService(
                _usageRepository.Object,
                _authorizationContext.Object,
                _systemRepository.Object,
                _referenceService.Object,
                _transactionManager.Object,
                _domainEvents.Object,
                _sensitiveDataLevelRepository.Object,
                _userContextMock.Object,
                new Mock<IItSystemUsageAttachedOptionRepository>().Object,
                _archivePeriodRepositoryMock.Object,
                _personalDataOptionsRepository.Object);
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
            Assert.Equal(new[] { itSystemUsage1, itSystemUsage2, itSystemUsage3 }, result.ToList());
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
            _userContextMock.Setup(x => x.OrganizationIds).Returns(new[] { itSystemUsage1.OrganizationId, itSystemUsage3.OrganizationId });

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
            var result = _sut.Query(subQuery1.Object, subQuery2.Object);

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
            var result = _sut.GetReadableItSystemUsageByUuid(uuid);

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
            var result = _sut.GetReadableItSystemUsageByUuid(uuid);

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
            var result = _sut.GetReadableItSystemUsageByUuid(uuid);

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
        public void Add_Returns_BadState_If_System_Is_Disabled()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage { ItSystemId = A<int>(), OrganizationId = A<int>() };
            var itSystem = new ItSystem() { Disabled = true }; //set system as disabled
            SetupRepositoryQueryWith(Enumerable.Empty<ItSystemUsage>());
            _authorizationContext.Setup(x => x.AllowCreate<ItSystemUsage>(itSystemUsage.OrganizationId, itSystemUsage)).Returns(true);
            _systemRepository.Setup(x => x.GetSystem(itSystemUsage.ItSystemId)).Returns(itSystem);
            _authorizationContext.Setup(x => x.AllowReads(itSystem)).Returns(true);

            //Act
            var result = _sut.Add(itSystemUsage);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.BadState, result.Error.FailureType);
            Assert.Equal("Cannot take disabled it-system into use", result.Error.Message.GetValueOrEmptyString());
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
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
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
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
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
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);
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
            _domainEvents.Verify(x => x.Raise(It.Is<EntityLifeCycleEvent<ItSystemUsage>>(ev => ev.Entity == itSystemUsage && ev.ChangeType == LifeCycleEventType.Deleting)));
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
        [InlineData(SensitiveDataLevel.SENSITIVEDATA)]
        [InlineData(SensitiveDataLevel.LEGALDATA)]
        public void RemoveSensitiveData_Returns_Ok(SensitiveDataLevel sensitiveDataLevel)
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);
            var usageSensitiveDataLevel = CreateSensitiveDataLevel(itSystemUsage, sensitiveDataLevel);
            itSystemUsage.SensitiveDataLevels.Add(usageSensitiveDataLevel);
            ExpectAllowModifyReturns(itSystemUsage, true);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);

            //Act
            var result = _sut.RemoveSensitiveDataLevel(itSystemUsage.Id, sensitiveDataLevel);

            //Assert
            Assert.True(result.Ok);
            var removedSensitiveData = result.Value;
            Assert.Equal(sensitiveDataLevel, removedSensitiveData.SensitivityDataLevel);
            _sensitiveDataLevelRepository.Verify(x => x.DeleteWithReferencePreload(usageSensitiveDataLevel), Times.Once);
        }


        [Fact]
        public void RemoveSensitiveData_Returns_Ok_And_Removes_PersonalData()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);
            var sensitiveDataLevel = SensitiveDataLevel.PERSONALDATA;
            var usageSensitiveDataLevel = CreateSensitiveDataLevel(itSystemUsage, sensitiveDataLevel);
            
            itSystemUsage.SensitiveDataLevels.Add(usageSensitiveDataLevel);

            var personalDataOption = new ItSystemUsagePersonalData
            {
                ItSystemUsage = itSystemUsage,
                PersonalData = A<GDPRPersonalDataOption>()
            };
            itSystemUsage.PersonalDataOptions.Add(personalDataOption);

            ExpectAllowModifyReturns(itSystemUsage, true);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);

            //Act
            var result = _sut.RemoveSensitiveDataLevel(itSystemUsage.Id, sensitiveDataLevel);

            //Assert
            Assert.True(result.Ok);
            var removedSensitiveData = result.Value;
            Assert.Equal(sensitiveDataLevel, removedSensitiveData.SensitivityDataLevel);
            _sensitiveDataLevelRepository.Verify(x => x.DeleteWithReferencePreload(usageSensitiveDataLevel), Times.Once);
            _personalDataOptionsRepository.Verify(
                x => x.RemoveRange(It.Is<IEnumerable<ItSystemUsagePersonalData>>(personalDataOptions =>
                    personalDataOptions.Contains(personalDataOption))), Times.Once);
        }

        private ItSystemUsageSensitiveDataLevel CreateSensitiveDataLevel(ItSystemUsage usage, SensitiveDataLevel sensitiveDataLevel)
        {
            return new ItSystemUsageSensitiveDataLevel()
            {
                ItSystemUsage = usage,
                SensitivityDataLevel = sensitiveDataLevel
            };
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

        [Fact]
        public void RemoveAllArchivePeriods_Returns_List_Of_Removed_Periods()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);
            var archivePeriods = new List<ArchivePeriod>()
            {
                CreateValidArchivePeriod(),
                CreateValidArchivePeriod()
            };
            itSystemUsage.ArchivePeriods = archivePeriods.ToList();

            ExpectAllowModifyReturns(itSystemUsage, true);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var removeResult = _sut.RemoveAllArchivePeriods(itSystemUsage.Id);

            //Assert
            Assert.True(removeResult.Ok);
            Assert.Equal(archivePeriods.Count(), removeResult.Value.Count());
            Assert.Empty(itSystemUsage.ArchivePeriods);
            for (var i = 0; i < archivePeriods.Count(); i++)
            {
                AssertArchivePeriod(archivePeriods[i], removeResult.Value.ToList()[i]);
                _archivePeriodRepositoryMock.Verify(x => x.DeleteWithReferencePreload(archivePeriods[i]), Times.Once);
            }
        }

        [Fact]
        public void RemoveAllArchivePeriods_Returns_NotFound()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);
            var archivePeriods = new List<ArchivePeriod>()
            {
                CreateValidArchivePeriod(),
                CreateValidArchivePeriod()
            };
            itSystemUsage.ArchivePeriods = archivePeriods.ToList();

            ExpectAllowModifyReturns(itSystemUsage, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var removeResult = _sut.RemoveAllArchivePeriods(itSystemUsage.Id);

            //Assert
            Assert.True(removeResult.Failed);
            Assert.Equal(OperationFailure.NotFound, removeResult.Error.FailureType);
        }

        [Fact]
        public void RemoveAllArchivePeriods_Returns_Forbidden()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);
            var archivePeriods = new List<ArchivePeriod>()
            {
                CreateValidArchivePeriod(),
                CreateValidArchivePeriod()
            };
            itSystemUsage.ArchivePeriods = archivePeriods.ToList();

            ExpectAllowModifyReturns(itSystemUsage, false);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var removeResult = _sut.RemoveAllArchivePeriods(itSystemUsage.Id);

            //Assert
            Assert.True(removeResult.Failed);
            Assert.Equal(OperationFailure.Forbidden, removeResult.Error.FailureType);
        }

        [Fact]
        public void AddArchivePeriod_Returns_ArchivePeriod()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);
            var archivePeriod = CreateValidArchivePeriod();

            ExpectAllowModifyReturns(itSystemUsage, true);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var addResult = _sut.AddArchivePeriod(itSystemUsage.Id, archivePeriod.StartDate, archivePeriod.EndDate, archivePeriod.UniqueArchiveId, archivePeriod.Approved);

            //Assert
            Assert.True(addResult.Ok);
            AssertArchivePeriod(archivePeriod, addResult.Value);
        }

        [Fact]
        public void AddArchivePeriod_Returns_NotFound()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);
            var archivePeriod = CreateValidArchivePeriod();

            ExpectAllowModifyReturns(itSystemUsage, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var addResult = _sut.AddArchivePeriod(itSystemUsage.Id, archivePeriod.StartDate, archivePeriod.EndDate, archivePeriod.UniqueArchiveId, archivePeriod.Approved);

            //Assert
            Assert.True(addResult.Failed);
            Assert.Equal(OperationFailure.NotFound, addResult.Error.FailureType);
        }

        [Fact]
        public void AddArchivePeriod_Returns_Forbidden()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);
            var archivePeriod = CreateValidArchivePeriod();

            ExpectAllowModifyReturns(itSystemUsage, false);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var addResult = _sut.AddArchivePeriod(itSystemUsage.Id, archivePeriod.StartDate, archivePeriod.EndDate, archivePeriod.UniqueArchiveId, archivePeriod.Approved);

            //Assert
            Assert.True(addResult.Failed);
            Assert.Equal(OperationFailure.Forbidden, addResult.Error.FailureType);
        }

        [Fact]
        public void AddArchivePeriod_Returns_BadInput_If_StartDate_Later_Than_EndDate()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);
            var startDate = A<DateTime>();
            var endDate = startDate.AddDays(-1);
            var archiveId = A<string>();
            var approved = A<bool>();

            ExpectAllowModifyReturns(itSystemUsage, true);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var addResult = _sut.AddArchivePeriod(itSystemUsage.Id, startDate, endDate, archiveId, approved);

            //Assert
            Assert.True(addResult.Failed);
            Assert.Equal(OperationFailure.BadInput, addResult.Error.FailureType);
        }

        [Fact]
        public void Can_TransferResponsibleUsage()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);

            var targetUnit = CreateOrganizationUnit(itSystemUsage.OrganizationId);
            itSystemUsage.Organization.OrgUnits.Add(targetUnit);
            itSystemUsage.ResponsibleUsage = CreateItSystemUsageOrgUnitUsage(itSystemUsage, CreateOrganizationUnit(itSystemUsage.OrganizationId));

            ExpectAllowModifyReturns(itSystemUsage, true);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var error = _sut.TransferResponsibleUsage(itSystemUsage.Id, targetUnit.Uuid);

            //Assert
            Assert.True(error.IsNone);
            _usageRepository.Verify(x => x.Update(itSystemUsage));
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void TransferResponsibleUsage_Returns_NotFound()
        {
            Test_Command_Which_Fails_With_Usage_NotFound(id => _sut.TransferResponsibleUsage(id, A<Guid>()));
        }

        [Fact]
        public void TransferResponsibleUsage_Returns_Forbidden()
        {
            Test_Command_Which_Fails_With_Usage_Insufficient_WriteAccess(id => _sut.TransferResponsibleUsage(id, A<Guid>()));
        }

        [Fact]
        public void Can_TransferRelevantUsage()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);

            var targetUnit = CreateOrganizationUnit(itSystemUsage.OrganizationId);
            var unit = CreateOrganizationUnit(itSystemUsage.OrganizationId);

            itSystemUsage.Organization.OrgUnits.Add(unit);
            itSystemUsage.Organization.OrgUnits.Add(targetUnit);

            itSystemUsage.UsedBy = new List<ItSystemUsageOrgUnitUsage>{ CreateItSystemUsageOrgUnitUsage(itSystemUsage, unit) };

            ExpectAllowModifyReturns(itSystemUsage, true);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var error = _sut.TransferRelevantUsage(itSystemUsage.Id, unit.Uuid, targetUnit.Uuid);

            //Assert
            Assert.True(error.IsNone);
            _usageRepository.Verify(x => x.Update(itSystemUsage));
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void TransferRelevantUsage_Returns_NotFound()
        {
            Test_Command_Which_Fails_With_Usage_NotFound(id => _sut.TransferRelevantUsage(id, A<Guid>(), A<Guid>()));
        }

        [Fact]
        public void TransferRelevantUsage_Returns_Forbidden()
        {
            Test_Command_Which_Fails_With_Usage_Insufficient_WriteAccess(id => _sut.TransferRelevantUsage(id, A<Guid>(), A<Guid>()));
        }

        [Fact]
        public void Can_RemoveResponsibleUsage()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);

            itSystemUsage.ResponsibleUsage = CreateItSystemUsageOrgUnitUsage(itSystemUsage, CreateOrganizationUnit(itSystemUsage.OrganizationId));

            ExpectAllowModifyReturns(itSystemUsage, true);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var error = _sut.RemoveResponsibleUsage(itSystemUsage.Id);

            //Assert
            Assert.True(error.IsNone);
            _usageRepository.Verify(x => x.Update(itSystemUsage));
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void RemoveResponsibleUsage_Returns_NotFound()
        {
            Test_Command_Which_Fails_With_Usage_NotFound(id => _sut.RemoveResponsibleUsage(id));
        }

        [Fact]
        public void RemoveResponsibleUsage_Returns_Forbidden()
        {
            Test_Command_Which_Fails_With_Usage_Insufficient_WriteAccess(id => _sut.RemoveResponsibleUsage(id));
        }

        [Fact]
        public void Can_RemoveRelevantUnit()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);

            var unit = CreateOrganizationUnit(itSystemUsage.OrganizationId);

            itSystemUsage.Organization.OrgUnits.Add(unit);

            itSystemUsage.UsedBy = new List<ItSystemUsageOrgUnitUsage> { CreateItSystemUsageOrgUnitUsage(itSystemUsage, unit) };

            ExpectAllowModifyReturns(itSystemUsage, true);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var error = _sut.RemoveRelevantUnit(itSystemUsage.Id, unit.Uuid);

            //Assert
            Assert.True(error.IsNone);
            _usageRepository.Verify(x => x.Update(itSystemUsage));
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void RemoveRelevantUnit_Returns_NotFound()
        {
            Test_Command_Which_Fails_With_Usage_NotFound(id => _sut.RemoveRelevantUnit(id, A<Guid>()));
        }

        [Fact]
        public void RemoveRelevantUnit_Returns_Forbidden()
        {
            Test_Command_Which_Fails_With_Usage_Insufficient_WriteAccess(id => _sut.RemoveRelevantUnit(id, A<Guid>()));
        }

        [Fact]
        public void Can_AddPersonalDataOption()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);

            itSystemUsage.SensitiveDataLevels = new List<ItSystemUsageSensitiveDataLevel> { new(){ItSystemUsage = itSystemUsage, SensitivityDataLevel = SensitiveDataLevel.PERSONALDATA} };

            var option = A<GDPRPersonalDataOption>();

            ExpectAllowModifyReturns(itSystemUsage, true);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var error = _sut.AddPersonalDataOption(itSystemUsage.Id, option);

            //Assert
            Assert.True(error.IsNone);
            _usageRepository.Verify(x => x.Update(itSystemUsage));
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void AddPersonalDataOption_Returns_NotFound()
        {
            Test_Command_Which_Fails_With_Usage_NotFound(id => _sut.AddPersonalDataOption(id, A<GDPRPersonalDataOption>()));
        }

        [Fact]
        public void AddPersonalDataOption_Returns_Forbidden()
        {
            Test_Command_Which_Fails_With_Usage_Insufficient_WriteAccess(id => _sut.AddPersonalDataOption(id, A<GDPRPersonalDataOption>()));
        }

        [Fact]
        public void Can_RemovePersonalDataOption()
        {
            //Arrange
            var itSystem = CreateItSystem();
            var itSystemUsage = CreateSystemUsage(A<int>(), itSystem);

            itSystemUsage.SensitiveDataLevels = new List<ItSystemUsageSensitiveDataLevel> { CreateSensitiveDataLevel(itSystemUsage, SensitiveDataLevel.PERSONALDATA) };

            var option = A<GDPRPersonalDataOption>();

            var personalDataOption = new ItSystemUsagePersonalData { ItSystemUsage = itSystemUsage, PersonalData = option };
            itSystemUsage.PersonalDataOptions = new List<ItSystemUsagePersonalData> { personalDataOption };
            
            ExpectAllowModifyReturns(itSystemUsage, true);
            _usageRepository.Setup(x => x.GetByKey(itSystemUsage.Id)).Returns(itSystemUsage);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var error = _sut.RemovePersonalDataOption(itSystemUsage.Id, option);

            //Assert
            Assert.True(error.IsNone);
            _usageRepository.Verify(x => x.Update(itSystemUsage));
            _personalDataOptionsRepository.Verify(x => x.Delete(personalDataOption));
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void RemovePersonalDataOption_Returns_NotFound()
        {
            Test_Command_Which_Fails_With_Usage_NotFound(id => _sut.RemovePersonalDataOption(id, A<GDPRPersonalDataOption>()));
        }
        
        [Fact]
        public void RemovePersonalDataOption_Returns_Forbidden()
        {
            Test_Command_Which_Fails_With_Usage_Insufficient_WriteAccess(id => _sut.RemovePersonalDataOption(id, A<GDPRPersonalDataOption>()));
        }

        //TODO: Get permissions - Read only, Read+ modify, Read+ modify+delete, None (forbidden), error

        private static void AssertArchivePeriod(ArchivePeriod expected, ArchivePeriod actual)
        {
            Assert.Equal(expected.StartDate, actual.StartDate);
            Assert.Equal(expected.EndDate, actual.EndDate);
            Assert.Equal(expected.Approved, actual.Approved);
            Assert.Equal(expected.UniqueArchiveId, actual.UniqueArchiveId);
        }

        private ArchivePeriod CreateValidArchivePeriod()
        {
            var startDate = A<DateTime>();
            return new ArchivePeriod()
            {
                Approved = A<bool>(),
                UniqueArchiveId = A<string>(),
                StartDate = startDate,
                EndDate = startDate.AddDays(1)
            };
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
                Organization = new Organization
                {
                    Id = organizationId
                },
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

        private OrganizationUnit CreateOrganizationUnit(int orgId)
        {
            return new OrganizationUnit()
            {
                Name = A<string>(),
                Id = A<int>(),
                Uuid = A<Guid>(),
                OrganizationId = orgId
            };
        }

        private ItSystemUsageOrgUnitUsage CreateItSystemUsageOrgUnitUsage(ItSystemUsage usage, OrganizationUnit organizationUnit)
        {
            return new ItSystemUsageOrgUnitUsage()
            {
                ItSystemUsage = usage,
                ItSystemUsageId = usage.Id,
                OrganizationUnit = organizationUnit,
                OrganizationUnitId = organizationUnit.Id
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


        /// <summary>
        /// Helper test to make it easy to cover the "Usage not found" case
        /// </summary>
        /// <param name="command"></param>
        private void Test_Command_Which_Fails_With_Usage_NotFound(Func<int, Maybe<OperationError>> command)
        {
            //Arrange
            var id = A<int>();
            ExpectGetUsageByKeyReturns(id, null);

            //Act
            var error = command(id);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
        }

        /// <summary>
        /// Helper test to make it easy to cover the "Missing Write access" case
        /// </summary>
        /// <param name="command"></param>
        private void Test_Command_Which_Fails_With_Usage_Insufficient_WriteAccess(Func<int, Maybe<OperationError>> command)
        {
            //Arrange
            var id = A<int>();
            var itSystemUsage = CreateSystemUsage(A<int>(), CreateItSystem());
            ExpectGetUsageByKeyReturns(id, itSystemUsage);
            ExpectAllowModifyReturns(itSystemUsage, false);

            //Act
            var result = command(id);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Forbidden, result.Value.FailureType);
        }
    }
}
