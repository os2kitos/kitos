using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Model.Interface;
using Core.ApplicationServices.OptionTypes;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.Interface;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;
using DataRow = Core.DomainModel.ItSystem.DataRow;

namespace Tests.Unit.Core.ApplicationServices
{
    public class ItInterfaceServiceTest : WithAutoFixture
    {
        private readonly ItInterfaceService _sut;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IItSystemRepository> _systemRepository;
        private readonly Mock<IDomainEvents> _domainEvents;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IGenericRepository<DataRow>> _dataRowRepository;
        private readonly Mock<IInterfaceRepository> _repository;
        private readonly Mock<IOrganizationalUserContext> _userContext;
        private readonly Mock<IOperationClock> _operationClock;
        private readonly Mock<IOptionResolver> _optionResolverMock;
        private readonly Mock<IOrganizationService> _organizationService;

        public ItInterfaceServiceTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _systemRepository = new Mock<IItSystemRepository>();
            _domainEvents = new Mock<IDomainEvents>();
            _transactionManager = new Mock<ITransactionManager>();
            _dataRowRepository = new Mock<IGenericRepository<DataRow>>();
            _userContext = new Mock<IOrganizationalUserContext>();
            _repository = new Mock<IInterfaceRepository>();
            _operationClock = new Mock<IOperationClock>();
            _optionResolverMock = new Mock<IOptionResolver>();
            _organizationService = new Mock<IOrganizationService>();
            _sut = new ItInterfaceService(
                _dataRowRepository.Object,
                _systemRepository.Object,
                _authorizationContext.Object,
                _transactionManager.Object,
                _domainEvents.Object,
                _repository.Object,
                _userContext.Object,
                _operationClock.Object,
                _optionResolverMock.Object,
                _organizationService.Object);
        }

        [Fact]
        public void UpdateExposingSystem_Returns_NotFound()
        {
            //Arrange
            var interfaceId = A<int>();
            ExpectGetInterfaceReturns(interfaceId, default(ItInterface));

            //Act
            var result = _sut.UpdateExposingSystem(interfaceId, A<int>());

            //Assert
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void UpdateExposingSystem_Returns_Forbidden_If_Modification_Access_To_Interface_Is_Denied()
        {
            //Arrange
            var interfaceId = A<int>();
            var itInterface = new ItInterface();
            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowModifyReturns(itInterface, false);

            //Act
            var result = _sut.UpdateExposingSystem(interfaceId, A<int>());

            //Assert
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void UpdateExposingSystem_Returns_BadInput_If_System_Id_Is_Invalid()
        {
            //Arrange
            var interfaceId = A<int>();
            var itInterface = new ItInterface();
            var newSystemId = A<int>();

            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowModifyReturns(itInterface, true);
            ExpectGetSystemReturns(newSystemId, default(ItSystem));

            //Act
            var result = _sut.UpdateExposingSystem(interfaceId, newSystemId);

            //Assert
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void UpdateExposingSystem_Returns_Forbidden_If_Read_Access_To_System_Is_Denied()
        {
            //Arrange
            var interfaceId = A<int>();
            var itInterface = new ItInterface();
            var newSystemId = A<int>();
            var itSystem = new ItSystem();

            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowModifyReturns(itInterface, true);
            ExpectGetSystemReturns(newSystemId, itSystem);
            ExpectAllowReadReturns(itSystem, false);

            //Act
            var result = _sut.UpdateExposingSystem(interfaceId, newSystemId);

            //Assert
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void UpdateExposingSystem_Returns_Ok_When_Existing_And_New_Is_None()
        {
            //Arrange
            var interfaceId = A<int>();
            var itInterface = new ItInterface();

            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowModifyReturns(itInterface, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var result = _sut.UpdateExposingSystem(interfaceId, null);

            //Assert that no changes were saved
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit(), Times.Never);
            _repository.Verify(x => x.Update(itInterface), Times.Never);
        }

        [Fact]
        public void UpdateExposingSystem_Returns_Ok_When_Existing_And_New_Are_Equal()
        {
            //Arrange
            var interfaceId = A<int>();
            var existingSystem = new ItSystem() { Id = A<int>() };
            var itInterface = new ItInterface() { ExhibitedBy = new ItInterfaceExhibit() { ItSystem = existingSystem, ItSystemId = existingSystem.Id } };
            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowModifyReturns(itInterface, true);
            ExpectGetSystemReturns(existingSystem.Id, existingSystem);
            ExpectAllowReadReturns(existingSystem, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var result = _sut.UpdateExposingSystem(interfaceId, existingSystem.Id);

            //Assert that no changes were saved
            Assert.True(result.Ok);
            transaction.Verify(x => x.Commit(), Times.Never);
            _repository.Verify(x => x.Update(itInterface), Times.Never);
        }

        [Fact]
        public void UpdateExposingSystem_Returns_Ok_With_Changes()
        {
            //Arrange
            var interfaceId = A<int>();
            var existingSystem = new ItSystem() { Id = A<int>() };
            var newSystem = new ItSystem() { Id = A<int>() };
            var itInterface = new ItInterface() { ExhibitedBy = new ItInterfaceExhibit() { ItSystem = existingSystem } };
            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowModifyReturns(itInterface, true);
            ExpectGetSystemReturns(newSystem.Id, newSystem);
            ExpectAllowReadReturns(newSystem, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var result = _sut.UpdateExposingSystem(interfaceId, newSystem.Id);

            //Assert that no changes were saved
            Assert.True(result.Ok);
            Assert.Same(newSystem, result.Value.ExhibitedBy.ItSystem);
            transaction.Verify(x => x.Commit(), Times.Once);
            _repository.Verify(x => x.Update(itInterface), Times.Once);
        }

        [Fact]
        public void Delete_Returns_NotFound()
        {
            //Arrange
            var interfaceId = A<int>();
            ExpectGetInterfaceReturns(interfaceId, default(ItInterface));

            //Act
            var result = _sut.Delete(interfaceId);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void Delete_Returns_Forbidden()
        {
            //Arrange
            var interfaceId = A<int>();
            var itInterface = new ItInterface() { InterfaceId = interfaceId };
            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowDeleteReturns(itInterface, false);

            //Act
            var result = _sut.Delete(interfaceId);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void Delete_Returns_Conflict_If_Interface_Is_Exhibited()
        {
            //Arrange
            var interfaceId = A<int>();
            var itInterface = new ItInterface
            {
                InterfaceId = interfaceId,
                ExhibitedBy = new ItInterfaceExhibit()
            };
            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowDeleteReturns(itInterface, true);

            //Act
            var result = _sut.Delete(interfaceId);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Conflict, result.Error);
        }

        [Fact]
        public void Delete_Clears_Exhibits_If_BreakBindingsIsTrue_Returns_Conflict_If_Interface_Is_Exhibited()
        {
            //Arrange
            var interfaceId = A<int>();
            var dataRow1 = new DataRow { Id = A<int>() };
            var dataRow2 = new DataRow { Id = A<int>() };
            var itInterface = new ItInterface
            {
                InterfaceId = interfaceId,
                ExhibitedBy = new ItInterfaceExhibit() { ItSystem = new ItSystem() },
                DataRows = new List<DataRow>()
                {
                dataRow1,
                dataRow2
            }
            };
            var transaction = new Mock<IDatabaseTransaction>();
            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowModifyReturns(itInterface, true);
            ExpectAllowDeleteReturns(itInterface, true);
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var result = _sut.Delete(interfaceId, true);

            //Assert
            Assert.True(result.Ok);
            VerifySuccessfulDeletion(result, itInterface, dataRow1, dataRow2, transaction, 2); //2 commits because first one is removing the exhibit and then the interface itself
        }

        [Fact]
        public void Delete_Returns_Ok_And_Raises_Domain_Event()
        {
            //Arrange
            var interfaceId = A<int>();
            var dataRow1 = new DataRow { Id = A<int>() };
            var dataRow2 = new DataRow { Id = A<int>() };
            var interfaceToDelete = new ItInterface
            {
                InterfaceId = interfaceId,
                DataRows = new List<DataRow>
                {
                    dataRow1,
                    dataRow2
                }
            };
            var transaction = new Mock<IDatabaseTransaction>();
            ExpectGetInterfaceReturns(interfaceId, interfaceToDelete);
            ExpectAllowDeleteReturns(interfaceToDelete, true);
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var result = _sut.Delete(interfaceId);

            //Assert
            VerifySuccessfulDeletion(result, interfaceToDelete, dataRow1, dataRow2, transaction, 1);
        }

        private void VerifySuccessfulDeletion(Result<ItInterface, OperationFailure> result, ItInterface interfaceToDelete, DataRow dataRow1, DataRow dataRow2,
            Mock<IDatabaseTransaction> transaction, int expectedCommitsToTransaction = 1)
        {
            Assert.True(result.Ok);
            _domainEvents.Verify(x => x.Raise(It.Is<EntityBeingDeletedEvent<ItInterface>>(d => d.Entity == interfaceToDelete)),
                Times.Once);
            _dataRowRepository.Verify(x => x.DeleteByKey(dataRow1.Id), Times.Once);
            _dataRowRepository.Verify(x => x.DeleteByKey(dataRow2.Id), Times.Once);
            _dataRowRepository.Verify(x => x.Save(), Times.Once);
            _repository.Verify(x => x.Delete(interfaceToDelete), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Exactly(expectedCommitsToTransaction));
        }

        [Fact]
        public void GetInterface_Returns_Interface()
        {
            //Arrange
            var interfaceUuid = A<Guid>();
            var itInterface = new ItInterface();

            _repository.Setup(x => x.GetInterface(interfaceUuid)).Returns(itInterface);
            _authorizationContext.Setup(x => x.AllowReads(itInterface)).Returns(true);

            //Act
            var businessTypeResult = _sut.GetInterface(interfaceUuid);

            //Assert
            Assert.True(businessTypeResult.Ok);
            Assert.Equal(itInterface, businessTypeResult.Value);
        }

        [Fact]
        public void GetInterface_Returns_Forbidden_If_Not_Read_Access()
        {
            //Arrange
            var interfaceUuid = A<Guid>();
            var itInterface = new ItInterface();

            _repository.Setup(x => x.GetInterface(interfaceUuid)).Returns(itInterface);
            _authorizationContext.Setup(x => x.AllowReads(itInterface)).Returns(false);

            //Act
            var businessTypeResult = _sut.GetInterface(interfaceUuid);

            //Assert
            Assert.True(businessTypeResult.Failed);
            Assert.Equal(OperationFailure.Forbidden, businessTypeResult.Error.FailureType);
        }

        [Fact]
        public void GetInterface_Returns_NotFound_If_No_Interface()
        {
            //Arrange
            var interfaceUuid = A<Guid>();

            _repository.Setup(x => x.GetInterface(interfaceUuid)).Returns(Maybe<ItInterface>.None);

            //Act
            var businessTypeResult = _sut.GetInterface(interfaceUuid);

            //Assert
            Assert.True(businessTypeResult.Failed);
            Assert.Equal(OperationFailure.NotFound, businessTypeResult.Error.FailureType);
        }

        [Fact]
        public void GetAvailableInterfaces_Returns_RightsholderInterfaces_If_User_Has_Rightsholderaccess()
        {
            //Arrange
            var interfaceUuid = A<Guid>();
            var org = new Organization()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var rightsHolderOrgs = new List<int>() { org.Id };
            var itInterface = new ItInterface()
            {
                Uuid = interfaceUuid,
                ExhibitedBy = new ItInterfaceExhibit()
                {
                    ItSystemId = A<int>(),
                    ItSystem = new ItSystem()
                    {
                        BelongsTo = org,
                        BelongsToId = org.Id
                    }
                }
            };

            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(CrossOrganizationDataReadAccessLevel.RightsHolder);
            _userContext.Setup(x => x.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess)).Returns(rightsHolderOrgs);
            _repository.Setup(x => x.GetInterfaces()).Returns(new List<ItInterface>() { itInterface }.AsQueryable());

            //Act
            var result = _sut.GetAvailableInterfaces();

            //Assert
            var interfaceResult = Assert.Single(result.ToList());
            Assert.Equal(interfaceUuid, interfaceResult.Uuid);
        }

        [Fact]
        public void GetAvailableInterfaces_Returns_Interfaces_Where_User_Has_Rightsholderaccess_Or_Other_Role_In_Owning_Org()
        {
            //Arrange
            var rightsHolderItInterfaceUuid = A<Guid>();
            var organizationItInterfaceUuid = A<Guid>();
            var rightsHolderOrg = new Organization()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var rightsHolderOrgs = new List<int>() { rightsHolderOrg.Id };
            var userRoleOrg = new Organization()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var userRoleOrgs = new List<int>() { userRoleOrg.Id };
            var rightsHolderItInterface = new ItInterface()
            {
                Uuid = rightsHolderItInterfaceUuid,
                ExhibitedBy = new ItInterfaceExhibit()
                {
                    ItSystemId = A<int>(),
                    ItSystem = new ItSystem()
                    {
                        BelongsTo = rightsHolderOrg,
                        BelongsToId = rightsHolderOrg.Id
                    }
                }
            };
            var organizationItInterface = new ItInterface()
            {
                Uuid = organizationItInterfaceUuid,
                OrganizationId = userRoleOrg.Id,
            };

            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(CrossOrganizationDataReadAccessLevel.RightsHolder);
            _userContext.Setup(x => x.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess)).Returns(rightsHolderOrgs);
            _userContext.Setup(x => x.OrganizationIds).Returns(userRoleOrgs);
            _repository.Setup(x => x.GetInterfaces()).Returns(new List<ItInterface>() { rightsHolderItInterface, organizationItInterface }.AsQueryable());

            //Act
            var result = _sut.GetAvailableInterfaces();

            //Assert
            Assert.Equal(2, result.Count());
            var rightsHolderItInterfaceResult = Assert.Single(result.Where(x => x.Uuid == rightsHolderItInterfaceUuid));
            Assert.Equal(rightsHolderItInterfaceUuid, rightsHolderItInterfaceResult.Uuid);
            var organizationItInterfaceResult = Assert.Single(result.Where(x => x.Uuid == organizationItInterfaceUuid));
            Assert.Equal(organizationItInterfaceUuid, organizationItInterfaceResult.Uuid);
        }

        [Fact]
        public void GetAvailableInterfaces_Returns_RightsholderInterfaces_From_Specific_Rightsholder_If_User_Has_Rightsholderaccess()
        {
            //Arrange
            var org1 = new Organization()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var org2 = new Organization()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var rightsHolderOrgs = new List<int>() {
                org1.Id,
                org2.Id
            };
            var itInterface1 = new ItInterface()
            {
                Uuid = A<Guid>(),
                ExhibitedBy = new ItInterfaceExhibit()
                {
                    ItSystemId = A<int>(),
                    ItSystem = new ItSystem()
                    {
                        BelongsTo = org1,
                        BelongsToId = org1.Id
                    }
                }
            };
            var itInterface2 = new ItInterface()
            {
                Uuid = A<Guid>(),
                ExhibitedBy = new ItInterfaceExhibit()
                {
                    ItSystemId = A<int>(),
                    ItSystem = new ItSystem()
                    {
                        BelongsTo = org2,
                        BelongsToId = org2.Id
                    }
                }
            };

            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(CrossOrganizationDataReadAccessLevel.RightsHolder);
            _userContext.Setup(x => x.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess)).Returns(rightsHolderOrgs);
            _repository.Setup(x => x.GetInterfaces()).Returns(new List<ItInterface>() { itInterface1, itInterface2 }.AsQueryable());

            var domainQuery = new Mock<IDomainQuery<ItInterface>>();
            domainQuery.Setup(x => x.Apply(It.IsAny<IQueryable<ItInterface>>())).Returns(new List<ItInterface>() { itInterface1 }.AsQueryable());

            var refinements = new List<IDomainQuery<ItInterface>>();

            refinements.Add(domainQuery.Object);

            //Act
            var result = _sut.GetAvailableInterfaces(refinements.ToArray());

            //Assert
            var interfaceResult = Assert.Single(result.ToList());
            Assert.Equal(itInterface1.Uuid, interfaceResult.Uuid);
        }

        [Theory]
        [InlineData(CrossOrganizationDataReadAccessLevel.All, 2)]
        [InlineData(CrossOrganizationDataReadAccessLevel.Public, 1)]
        [InlineData(CrossOrganizationDataReadAccessLevel.None, 0)]
        public void GetAvailableInterfaces_Returns_Public_Interfaces_If_User_Has_Access_Level_Public(CrossOrganizationDataReadAccessLevel accessLevel, int expectedNumberOfInterfaces)
        {
            //Arrange
            var interfaceUuid = A<Guid>();
            var org = new Organization()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var rightsHolderOrgs = new List<int>() { org.Id };
            var publicItInterface = new ItInterface()
            {
                Uuid = interfaceUuid,
                AccessModifier = AccessModifier.Public
            };
            var localItInterface = new ItInterface()
            {
                Uuid = interfaceUuid,
                AccessModifier = AccessModifier.Local
            };

            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(accessLevel);
            _repository.Setup(x => x.GetInterfaces()).Returns(new List<ItInterface>() { publicItInterface, localItInterface }.AsQueryable());

            //Act
            var result = _sut.GetAvailableInterfaces();

            //Assert
            Assert.Equal(expectedNumberOfInterfaces, result.Count());
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void CreateNewItInterface_Returns_Created_Interface(bool withSpecifiedUuid, bool withAccessModifier)
        {
            //Arrange
            var name = A<string>();
            var itInterfaceId = A<string>();
            var uuid = A<Guid>();
            var accessModifier = A<AccessModifier>();
            var org = new Organization()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };

            var transaction = SetupTransaction();
            ExpectAllowCreate(org.Id, true);
            _repository.Setup(x => x.GetInterface(uuid)).Returns(Maybe<ItInterface>.None);
            _repository.Setup(x => x.GetInterfaces()).Returns(new List<ItInterface>().AsQueryable()); //Returns nothing so no conflicts exists
            _operationClock.Setup(x => x.Now).Returns(DateTime.Now);
            _authorizationContext.Setup(x => x.HasPermission(It.IsAny<CreateEntityWithVisibilityPermission>())).Returns(true);

            //Act
            var createdInterfaceResult = _sut.CreateNewItInterface(org.Id, name, itInterfaceId, withSpecifiedUuid ? uuid : null, withAccessModifier ? accessModifier : null);

            //Assert
            Assert.True(createdInterfaceResult.Ok);

            if (withSpecifiedUuid)
                Assert.Equal(uuid, createdInterfaceResult.Value.Uuid);
            else
                Assert.NotEqual(Guid.Empty, createdInterfaceResult.Value.Uuid);

            if (withAccessModifier)
                Assert.Equal(accessModifier, createdInterfaceResult.Value.AccessModifier);
            else
                Assert.Equal(AccessModifier.Public, createdInterfaceResult.Value.AccessModifier); // Defaults to Public access modifier

            Assert.Equal(name, createdInterfaceResult.Value.Name);
            Assert.Equal(itInterfaceId, createdInterfaceResult.Value.ItInterfaceId);



            _domainEvents.Verify(x => x.Raise(It.IsAny<EntityCreatedEvent<ItInterface>>()), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Theory]
        [InlineData(null, "Valid ItInterfaceId")]
        [InlineData("", "Valid ItInterfaceId")]
        [InlineData("Valid Name", null)]
        public void CreateNewItInterface_Returns_BadInput_If_Name_Or_ItInterfaceId_Is_Not_Valid(string name, string itInterfaceId)
        {
            //Arrange
            var org = new Organization()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };

            var transaction = SetupTransaction();
            ExpectAllowCreate(org.Id, true);

            //Act
            var createdInterfaceResult = _sut.CreateNewItInterface(org.Id, name, itInterfaceId);

            //Assert
            Assert.True(createdInterfaceResult.Failed);
            Assert.Equal(OperationFailure.BadInput, createdInterfaceResult.Error.FailureType);

            _domainEvents.Verify(x => x.Raise(It.IsAny<EntityCreatedEvent<ItInterface>>()), Times.Never);
        }

        [Fact]
        public void CreateNewItInterface_Returns_Conflict_When_Name_And_ItInterfaceId_Combination_Already_Exists_In_Organization()
        {
            //Arrange
            var name = A<string>();
            var itInterfaceId = A<string>();
            var org = new Organization()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };

            var transaction = SetupTransaction();
            ExpectAllowCreate(org.Id, true);
            _repository.Setup(x => x.GetInterface(It.IsAny<Guid>())).Returns(Maybe<ItInterface>.None);
            _repository.Setup(x => x.GetInterfaces()).Returns(new List<ItInterface>() { new ItInterface() { OrganizationId = org.Id, Name = name, ItInterfaceId = itInterfaceId } }.AsQueryable()); //Returns interface to conflict with
            _operationClock.Setup(x => x.Now).Returns(DateTime.Now);

            //Act
            var createdInterfaceResult = _sut.CreateNewItInterface(org.Id, name, itInterfaceId);

            //Assert
            Assert.True(createdInterfaceResult.Failed);
            Assert.Equal(OperationFailure.Conflict, createdInterfaceResult.Error.FailureType);

            _domainEvents.Verify(x => x.Raise(It.IsAny<EntityCreatedEvent<ItInterface>>()), Times.Never);
        }

        [Fact]
        public void CreateNewItInterface_Returns_Conflict_When_Interface_With_Uuid_Already_Exists()
        {
            //Arrange
            var name = A<string>();
            var itInterfaceId = A<string>();
            var uuid = A<Guid>();
            var org = new Organization()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };

            var transaction = SetupTransaction();
            ExpectAllowCreate(org.Id, true);
            _repository.Setup(x => x.GetInterface(uuid)).Returns(new ItInterface() { Uuid = uuid }); //Returns interface with uuid already applied so conflict exists
            _repository.Setup(x => x.GetInterfaces()).Returns(new List<ItInterface>().AsQueryable()); //Returns nothing so no conflicts exists
            _operationClock.Setup(x => x.Now).Returns(DateTime.Now);

            //Act
            var createdInterfaceResult = _sut.CreateNewItInterface(org.Id, name, itInterfaceId, uuid);

            //Assert
            Assert.True(createdInterfaceResult.Failed);
            Assert.Equal(OperationFailure.Conflict, createdInterfaceResult.Error.FailureType);

            _domainEvents.Verify(x => x.Raise(It.IsAny<EntityCreatedEvent<ItInterface>>()), Times.Never);
        }

        [Fact]
        public void CreateNewItInterface_Returns_Forbidden_When_Not_Allowed_To_Create()
        {
            //Arrange
            var name = A<string>();
            var itInterfaceId = A<string>();
            var uuid = A<Guid>();
            var org = new Organization()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };

            ExpectAllowCreate(org.Id, false);

            //Act
            var createdInterfaceResult = _sut.CreateNewItInterface(org.Id, name, itInterfaceId, uuid);

            //Assert
            Assert.True(createdInterfaceResult.Failed);
            Assert.Equal(OperationFailure.Forbidden, createdInterfaceResult.Error.FailureType);

            _domainEvents.Verify(x => x.Raise(It.IsAny<EntityCreatedEvent<ItInterface>>()), Times.Never);
        }

        [Fact]
        public void UpdateVersion_Returns_Updated_Interface()
        {
            Test_Command_Which_Mutates_ItInterface_With_Success(itInterface =>
            {
                //Arrange
                var newVersion = A<string>();

                //Act
                var updatedInterface = _sut.UpdateVersion(itInterface.Id, newVersion);

                //Assert
                Assert.Equal(newVersion, updatedInterface.Value.Version);

                return updatedInterface; // Return to complete generic assertions
            });
        }

        [Fact]
        public void UpdateVersion_Returns_Forbidden()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_Forbidden(itInterface => _sut.UpdateVersion(itInterface.Id, A<string>()));
        }

        [Fact]
        public void UpdateVersion_Returns_NotFound()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_NotFound(itInterface => _sut.UpdateVersion(itInterface.Id, A<string>()));
        }

        [Fact]
        public void UpdateAccessModifier_Returns_Updated_Interface()
        {
            Test_Command_Which_Mutates_ItInterface_With_Success(itInterface =>
            {
                //Arrange
                var newAccessModifier = itInterface.AccessModifier == AccessModifier.Local ? AccessModifier.Public : AccessModifier.Local;
                _authorizationContext.Setup(x => x.HasPermission(It.Is<VisibilityControlPermission>(p => p.Target == itInterface))).Returns(true);

                //Act
                var updatedInterface = _sut.UpdateAccessModifier(itInterface.Id, newAccessModifier);

                //Assert
                Assert.Equal(newAccessModifier, updatedInterface.Value.AccessModifier);

                return updatedInterface; // Return to complete generic assertions
            });
        }

        [Fact]
        public void UpdateAccessModifier_Fails_If_Not_Visibility_Control_Permission()
        {
            Test_Command_Which_Fails_Mutating_ItInterface(itInterface =>
            {
                var newAccessModifier = itInterface.AccessModifier == AccessModifier.Local ? AccessModifier.Public : AccessModifier.Local;
                _authorizationContext.Setup(x => x.HasPermission(It.Is<VisibilityControlPermission>(p => p.Target == itInterface))).Returns(false);

                return _sut.UpdateAccessModifier(itInterface.Id, newAccessModifier);
            }, error => Assert.Equal(OperationFailure.Forbidden, error.FailureType));
        }

        [Fact]
        public void UpdateAccessModifier_Returns_Forbidden()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_Forbidden(itInterface => _sut.UpdateAccessModifier(itInterface.Id, A<AccessModifier>()));
        }

        [Fact]
        public void UpdateAccessModifier_Returns_NotFound()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_NotFound(itInterface => _sut.UpdateAccessModifier(itInterface.Id, A<AccessModifier>()));
        }

        [Fact]
        public void UpdateInterfaceType_Returns_Updated_Interface()
        {
            Test_Command_Which_Mutates_ItInterface_With_Success(itInterface =>
            {
                //Arrange
                var interfaceType = new InterfaceType { Uuid = A<Guid>() };
                _optionResolverMock
                    .Setup(x => x.GetOptionType<ItInterface, InterfaceType>(itInterface.Organization.Uuid,
                        interfaceType.Uuid)).Returns((interfaceType, true));

                //Act
                var updatedInterface = _sut.UpdateInterfaceType(itInterface.Id, interfaceType.Uuid);

                //Assert
                Assert.Equal(interfaceType, updatedInterface.Value.Interface);

                return updatedInterface; // Return to complete generic assertions
            });
        }

        [Fact]
        public void Cannot_UpdateInterfaceType_If_InterfaceType_Is_Not_Available()
        {
            Test_Command_Which_Fails_Mutating_ItInterface(itInterface =>
            {
                var interfaceType = new InterfaceType { Uuid = A<Guid>() };
                _optionResolverMock
                    .Setup(x => x.GetOptionType<ItInterface, InterfaceType>(itInterface.Organization.Uuid,
                        interfaceType.Uuid)).Returns((interfaceType, false));

                return _sut.UpdateInterfaceType(itInterface.Id, interfaceType.Uuid);
            }, error => Assert.Equal(OperationFailure.BadInput, error.FailureType));
        }

        [Fact]
        public void Cannot_UpdateInterfaceType_If_InterfaceType_Is_Not_Found()
        {
            var error = A<OperationError>();
            Test_Command_Which_Fails_Mutating_ItInterface(itInterface =>
            {
                var interfaceType = new InterfaceType { Uuid = A<Guid>() };
                _optionResolverMock
                    .Setup(x => x.GetOptionType<ItInterface, InterfaceType>(itInterface.Organization.Uuid,
                        interfaceType.Uuid)).Returns(error);

                return _sut.UpdateInterfaceType(itInterface.Id, interfaceType.Uuid);
            }, error => Assert.Equal(error.FailureType, error.FailureType));
        }

        [Fact]
        public void UpdateInterfaceType_Returns_Forbidden()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_Forbidden(itInterface => _sut.UpdateInterfaceType(itInterface.Id, A<Guid>()));
        }

        [Fact]
        public void UpdateInterfaceType_Returns_NotFound()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_NotFound(itInterface => _sut.UpdateInterfaceType(itInterface.Id, A<Guid>()));
        }

        [Fact]
        public void ReplaceInterfaceData_Returns_Updated_Interface()
        {
            Test_Command_Which_Mutates_ItInterface_With_Success(itInterface =>
            {
                //Arrange

                var dataType = new DataType() { Uuid = A<Guid>() };
                var rows = new[]
                {
                    new ItInterfaceDataWriteModel(A<string>(),dataType.Uuid),
                    new ItInterfaceDataWriteModel(A<string>(),null),
                };

                ExpectResolveDataTypeOptionReturns(itInterface, dataType.Uuid, (dataType, true));

                //Act
                var updatedInterface = _sut.ReplaceInterfaceData(itInterface.Id, rows);

                //Assert
                Assert.Equivalent(rows, updatedInterface.Value.DataRows.Select(x => new ItInterfaceDataWriteModel(x.Data, x.DataType?.Uuid)));

                return updatedInterface; // Return to complete generic assertions
            });
        }

        [Fact]
        public void Cannot_ReplaceInterfaceData_If_DataType_Is_Not_Available()
        {
            Test_Command_Which_Fails_Mutating_ItInterface(itInterface =>
            {
                var dataType = new DataType() { Uuid = A<Guid>() };
                var rows = new[]
                {
                    new ItInterfaceDataWriteModel(A<string>(),dataType.Uuid),
                };

                ExpectResolveDataTypeOptionReturns(itInterface, dataType.Uuid, (dataType, false));

                return _sut.ReplaceInterfaceData(itInterface.Id, rows);
            }, error => Assert.Equal(OperationFailure.BadInput, error.FailureType));
        }

        [Fact]
        public void Cannot_ReplaceInterfaceData_If_DataType_Is_Not_Found()
        {
            var error = A<OperationError>();
            Test_Command_Which_Fails_Mutating_ItInterface(itInterface =>
            {
                var dataType = new DataType() { Uuid = A<Guid>() };
                var rows = new[]
                {
                    new ItInterfaceDataWriteModel(A<string>(),dataType.Uuid),
                };

                ExpectResolveDataTypeOptionReturns(itInterface, dataType.Uuid, error);

                return _sut.ReplaceInterfaceData(itInterface.Id, rows);
            }, error => Assert.Equal(error.FailureType, error.FailureType));
        }

        [Fact]
        public void ReplaceInterfaceData_Returns_Forbidden()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_Forbidden(itInterface => _sut.ReplaceInterfaceData(itInterface.Id, Many<ItInterfaceDataWriteModel>()));
        }

        [Fact]
        public void ReplaceInterfaceData_Returns_NotFound()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_NotFound(itInterface => _sut.ReplaceInterfaceData(itInterface.Id, Many<ItInterfaceDataWriteModel>()));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddInterfaceData_Returns_Created_DataRow(bool withDataType)
        {
            Test_Command_Which_Mutates_ItInterface_With_Success(itInterface =>
            {
                //Arrange
                var dataType = new DataType() { Uuid = A<Guid>() };
                var input = new ItInterfaceDataWriteModel(A<string>(), withDataType ? dataType.Uuid : null);

                if (withDataType)
                {
                    ExpectResolveDataTypeOptionReturns(itInterface, dataType.Uuid, (dataType, true));

                }

                //Act
                var result = _sut.AddInterfaceData(itInterface.Id, input);

                //Assert
                Assert.True(result.Ok);
                var dataRow = result.Value;
                Assert.Equal(input.DataDescription, dataRow.Data);
                Assert.Equal(input.DataTypeUuid, dataRow.DataType?.Uuid);
                Assert.True(itInterface.DataRows.Contains(dataRow));
                return itInterface;
            });
        }

        [Fact]
        public void Cannot_AddInterfaceData_If_DataType_Is_Not_Available()
        {
            Test_Command_Which_Fails_Mutating_ItInterface(itInterface =>
            {
                var dataType = new DataType() { Uuid = A<Guid>() };
                var input = new ItInterfaceDataWriteModel(A<string>(), dataType.Uuid);

                ExpectResolveDataTypeOptionReturns(itInterface, dataType.Uuid, (dataType, false));

                return _sut.AddInterfaceData(itInterface.Id, input).Match<Result<ItInterface, OperationError>>(_ => itInterface, error => error);
            }, error => Assert.Equal(OperationFailure.BadInput, error.FailureType));
        }

        [Fact]
        public void Cannot_AddInterfaceData_If_DataType_Is_Not_Found()
        {
            var expectedError = A<OperationError>();
            Test_Command_Which_Fails_Mutating_ItInterface(itInterface =>
            {
                var dataType = new DataType() { Uuid = A<Guid>() };
                var input = new ItInterfaceDataWriteModel(A<string>(), dataType.Uuid);

                ExpectResolveDataTypeOptionReturns(itInterface, dataType.Uuid, expectedError);

                return _sut.AddInterfaceData(itInterface.Id, input).Match<Result<ItInterface, OperationError>>(_ => itInterface, operationError => operationError);
            }, error => Assert.Equal(expectedError.FailureType, error.FailureType));
        }

        [Fact]
        public void AddInterfaceData_Returns_Forbidden()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_Forbidden(itInterface => _sut.AddInterfaceData(itInterface.Id, A<ItInterfaceDataWriteModel>()).Match<Result<ItInterface, OperationError>>(_ => itInterface, error => error));
        }

        [Fact]
        public void AddInterfaceData_Returns_NotFound()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_NotFound(itInterface => _sut.AddInterfaceData(itInterface.Id, A<ItInterfaceDataWriteModel>()).Match<Result<ItInterface, OperationError>>(_ => itInterface, error => error));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateInterfaceData_Returns_Updated_DataRow(bool withDataType)
        {
            Test_Command_Which_Mutates_ItInterface_With_Success(itInterface =>
            {
                //Arrange
                var expectedUpdatedRow = new DataRow()
                {
                    Uuid = A<Guid>(),
                    Data = A<string>(),
                    DataType = new DataType() { Uuid = A<Guid>() }
                };
                itInterface.DataRows.Add(expectedUpdatedRow);
                itInterface.DataRows.Add(new DataRow());
                var dataType = new DataType() { Uuid = A<Guid>() };
                var input = new ItInterfaceDataWriteModel(A<string>(), withDataType ? dataType.Uuid : null);

                if (withDataType)
                {
                    ExpectResolveDataTypeOptionReturns(itInterface, dataType.Uuid, (dataType, true));

                }

                //Act
                var result = _sut.UpdateInterfaceData(itInterface.Id, expectedUpdatedRow.Uuid, input);

                //Assert
                Assert.True(result.Ok);
                var dataRow = result.Value;
                Assert.Same(expectedUpdatedRow, dataRow);
                Assert.Equal(input.DataDescription, dataRow.Data);
                Assert.Equal(input.DataTypeUuid, dataRow.DataType?.Uuid);
                return itInterface;
            });
        }

        [Fact]
        public void Cannot_UpdateInterfaceData_If_DataType_Is_Not_Available()
        {
            Test_Command_Which_Fails_Mutating_ItInterface(itInterface =>
            {
                //Arrange
                var expectedUpdatedRow = new DataRow()
                {
                    Uuid = A<Guid>(),
                    Data = A<string>(),
                    DataType = new DataType() { Uuid = A<Guid>() }
                };
                itInterface.DataRows.Add(expectedUpdatedRow);
                itInterface.DataRows.Add(new DataRow());
                var dataType = new DataType() { Uuid = A<Guid>() };
                var input = new ItInterfaceDataWriteModel(A<string>(), dataType.Uuid);

                ExpectResolveDataTypeOptionReturns(itInterface, dataType.Uuid, (dataType, false));

                return _sut.UpdateInterfaceData(itInterface.Id, expectedUpdatedRow.Uuid, input).Match<Result<ItInterface, OperationError>>(_ => itInterface, error => error);
            }, error => Assert.Equal(OperationFailure.BadInput, error.FailureType));
        }

        [Fact]
        public void Cannot_UpdateInterfaceData_If_DataType_Is_Not_Found()
        {
            var expectedError = A<OperationError>();
            Test_Command_Which_Fails_Mutating_ItInterface(itInterface =>
            {
                //Arrange
                var expectedUpdatedRow = new DataRow()
                {
                    Uuid = A<Guid>(),
                    Data = A<string>(),
                    DataType = new DataType() { Uuid = A<Guid>() }
                };
                itInterface.DataRows.Add(expectedUpdatedRow);
                itInterface.DataRows.Add(new DataRow());
                var dataType = new DataType() { Uuid = A<Guid>() };
                var input = new ItInterfaceDataWriteModel(A<string>(), dataType.Uuid);

                ExpectResolveDataTypeOptionReturns(itInterface, dataType.Uuid, expectedError);

                return _sut.UpdateInterfaceData(itInterface.Id, expectedUpdatedRow.Uuid, input).Match<Result<ItInterface, OperationError>>(_ => itInterface, error => error);
            }, error => Assert.Equal(expectedError.FailureType, error.FailureType));
        }

        [Fact]
        public void Cannot_UpdateInterfaceData_If_DataRow_Is_Not_Found()
        {
            Test_Command_Which_Fails_Mutating_ItInterface(itInterface =>
            {
                //Arrange
                itInterface.DataRows.Add(new DataRow());
                var input = new ItInterfaceDataWriteModel(A<string>(), null);

                return _sut.UpdateInterfaceData(itInterface.Id, A<Guid>(), input).Match<Result<ItInterface, OperationError>>(_ => itInterface, error => error);
            }, error => Assert.Equal(OperationFailure.BadInput, error.FailureType));
        }

        [Fact]
        public void UpdateInterfaceData_Returns_Forbidden()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_Forbidden(itInterface => _sut.UpdateInterfaceData(itInterface.Id, A<Guid>(), A<ItInterfaceDataWriteModel>()).Match<Result<ItInterface, OperationError>>(_ => itInterface, error => error));
        }

        [Fact]
        public void UpdateInterfaceData_Returns_NotFound()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_NotFound(itInterface => _sut.UpdateInterfaceData(itInterface.Id, A<Guid>(), A<ItInterfaceDataWriteModel>()).Match<Result<ItInterface, OperationError>>(_ => itInterface, error => error));
        }

        [Fact]
        public void DeleteInterfaceData_Returns_Updated_DataRow()
        {
            Test_Command_Which_Mutates_ItInterface_With_Success(itInterface =>
            {
                //Arrange
                var rowToDelete = new DataRow()
                {
                    Uuid = A<Guid>(),
                    Data = A<string>(),
                    DataType = new DataType() { Uuid = A<Guid>() }
                };
                itInterface.DataRows.Add(rowToDelete);
                itInterface.DataRows.Add(new DataRow());

                //Act
                var result = _sut.DeleteInterfaceData(itInterface.Id, rowToDelete.Uuid);

                //Assert
                Assert.True(result.Ok);
                var dataRow = result.Value;
                Assert.Same(rowToDelete, dataRow);
                Assert.False(itInterface.DataRows.Contains(dataRow));
                _dataRowRepository.Verify(x => x.Delete(rowToDelete), Times.Once());
                return itInterface;
            });
        }

        [Fact]
        public void Cannot_DeleteInterfaceData_If_DataRow_Is_Not_Found()
        {
            Test_Command_Which_Fails_Mutating_ItInterface(itInterface =>
            {
                //Arrange
                itInterface.DataRows.Add(new DataRow());

                return _sut.DeleteInterfaceData(itInterface.Id, A<Guid>()).Match<Result<ItInterface, OperationError>>(_ => itInterface, error => error);
            }, error => Assert.Equal(OperationFailure.BadInput, error.FailureType));
        }

        [Fact]
        public void DeleteInterfaceData_Returns_Forbidden()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_Forbidden(itInterface => _sut.DeleteInterfaceData(itInterface.Id, A<Guid>()).Match<Result<ItInterface, OperationError>>(_ => itInterface, error => error));
        }

        [Fact]
        public void DeleteInterfaceData_Returns_NotFound()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_NotFound(itInterface => _sut.DeleteInterfaceData(itInterface.Id, A<Guid>()).Match<Result<ItInterface, OperationError>>(_ => itInterface, error => error));
        }

        [Fact]
        public void UpdateDescription_Returns_Updated_Interface()
        {
            Test_Command_Which_Mutates_ItInterface_With_Success(itInterface =>
            {
                //Arrange
                var newDescription = A<string>();

                //Act
                var updatedInterface = _sut.UpdateDescription(itInterface.Id, newDescription);

                //Assert
                Assert.Equal(newDescription, updatedInterface.Value.Description);

                return updatedInterface; // Return to complete generic assertions
            });
        }

        [Fact]
        public void UpdateDescription_Returns_Forbidden()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_Forbidden(itInterface => _sut.UpdateDescription(itInterface.Id, A<string>()));
        }

        [Fact]
        public void UpdateDescription_Returns_NotFound()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_NotFound(itInterface => _sut.UpdateDescription(itInterface.Id, A<string>()));
        }

        [Fact]
        public void UpdateNote_Returns_Updated_Interface()
        {
            Test_Command_Which_Mutates_ItInterface_With_Success(itInterface =>
            {
                //Arrange
                var newNote = A<string>();

                //Act
                var updatedInterface = _sut.UpdateNote(itInterface.Id, newNote);

                //Assert
                Assert.Equal(newNote, updatedInterface.Value.Note);

                return updatedInterface; // Return to complete generic assertions
            });
        }

        [Fact]
        public void UpdateNote_Returns_Forbidden()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_Forbidden(itInterface => _sut.UpdateNote(itInterface.Id, A<string>()));
        }

        [Fact]
        public void UpdateNote_Returns_NotFound()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_NotFound(itInterface => _sut.UpdateNote(itInterface.Id, A<string>()));
        }

        [Fact]
        public void UpdateUrlReference_Returns_Updated_Interface()
        {
            Test_Command_Which_Mutates_ItInterface_With_Success(itInterface =>
            {
                //Arrange
                var newUrl = A<string>();

                //Act
                var updatedInterface = _sut.UpdateUrlReference(itInterface.Id, newUrl);

                //Assert
                Assert.Equal(newUrl, updatedInterface.Value.Url);

                return updatedInterface; // Return to complete generic assertions
            });
        }

        [Fact]
        public void UpdateUrlReference_Returns_Forbidden()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_Forbidden(itInterface => _sut.UpdateUrlReference(itInterface.Id, A<string>()));
        }

        [Fact]
        public void UpdateUrlReference_Returns_NotFound()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_NotFound(itInterface => _sut.UpdateUrlReference(itInterface.Id, A<string>()));
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void UpdateNameAndInterfaceId_Returns_Updated_Interface(bool withNewName, bool withNewItInterfaceId)
        {
            Test_Command_Which_Mutates_ItInterface_With_Success(itInterface =>
            {
                //Arrange
                var newItInterfaceId = withNewItInterfaceId ? A<string>() : itInterface.ItInterfaceId;
                var newName = withNewName ? A<string>() : itInterface.Name;

                _repository.Setup(x => x.GetInterfaces()).Returns(new List<ItInterface>().AsQueryable()); // Return nothing so no conflicts are found.

                //Act
                var updatedInterface = _sut.UpdateNameAndInterfaceId(itInterface.Id, newName, newItInterfaceId);

                //Assert
                Assert.Equal(newItInterfaceId, updatedInterface.Value.ItInterfaceId);
                Assert.Equal(newName, updatedInterface.Value.Name);

                return updatedInterface; // Return to complete generic assertions
            });
        }

        [Fact]
        public void UpdateNameAndInterfaceId_Returns_Forbidden()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_Forbidden(itInterface => _sut.UpdateNameAndInterfaceId(itInterface.Id, A<string>(), A<string>()));
        }

        [Fact]
        public void UpdateNameAndInterfaceId_Returns_NotFound()
        {
            Test_Command_Which_Mutates_ItInterface_With_Failure_NotFound(itInterface => _sut.UpdateNameAndInterfaceId(itInterface.Id, A<string>(), A<string>()));
        }

        [Fact]
        public void UpdateInterfaceId_Returns_Conflict()
        {
            //Arrange
            var itInterface = CreateInterfaceWithAllBasicPropertiesSet();
            var newItInterfaceId = A<string>();
            var newName = A<string>();

            var transaction = SetupTransaction();
            _repository.Setup(x => x.GetInterface(itInterface.Id)).Returns(itInterface);
            _authorizationContext.Setup(x => x.AllowModify(itInterface)).Returns(true);
            _repository.Setup(x => x.GetInterfaces()).Returns(
                new List<ItInterface>() {
                    new ItInterface() { OrganizationId = itInterface.OrganizationId, Name = newName, ItInterfaceId = newItInterfaceId }
                }.AsQueryable()); //Returns interface with same org, name and new ItInterfaceId

            //Act
            var updated = _sut.UpdateNameAndInterfaceId(itInterface.Id, newName, newItInterfaceId);

            //Assert
            Assert.True(updated.Failed);
            Assert.Equal(OperationFailure.Conflict, updated.Error.FailureType);
            _domainEvents.Verify(x => x.Raise(It.IsAny<EntityUpdatedEvent<ItInterface>>()), Times.Never);
        }

        [Fact]
        public void Deactivate_Returns_Ok()
        {
            //Arrange
            var itInterface = CreateInterfaceWithAllBasicPropertiesSet();
            var transaction = SetupTransaction();
            _repository.Setup(x => x.GetInterface(itInterface.Id)).Returns(itInterface);
            _authorizationContext.Setup(x => x.AllowModify(itInterface)).Returns(true);

            //Act
            var result = _sut.Deactivate(itInterface.Id);

            //Assert
            Assert.True(result.Ok);
            Assert.True(result.Select(x => x.Disabled).Value);

            _domainEvents.Verify(x => x.Raise(It.IsAny<EnabledStatusChanged<ItInterface>>()), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
            _repository.Verify(x => x.Update(It.IsAny<ItInterface>()), Times.Once);
        }

        [Fact]
        public void Cannot_Deactivate_If_No_Access()
        {
            //Arrange
            var itInterface = CreateInterfaceWithAllBasicPropertiesSet();
            var transaction = SetupTransaction();
            _repository.Setup(x => x.GetInterface(itInterface.Id)).Returns(itInterface);
            _authorizationContext.Setup(x => x.AllowModify(itInterface)).Returns(false);

            //Act
            var result = _sut.Deactivate(itInterface.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);

            _domainEvents.Verify(x => x.Raise(It.IsAny<EnabledStatusChanged<ItInterface>>()), Times.Never);
            transaction.Verify(x => x.Commit(), Times.Never);
            _repository.Verify(x => x.Update(It.IsAny<ItInterface>()), Times.Never);
        }

        [Fact]
        public void Cannot_Deactivate_If_Not_Found()
        {
            //Arrange
            var itInterface = CreateInterfaceWithAllBasicPropertiesSet();
            var transaction = SetupTransaction();
            _repository.Setup(x => x.GetInterface(itInterface.Id)).Returns(Maybe<ItInterface>.None);

            //Act
            var result = _sut.Deactivate(itInterface.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);

            _domainEvents.Verify(x => x.Raise(It.IsAny<EnabledStatusChanged<ItInterface>>()), Times.Never);
            transaction.Verify(x => x.Commit(), Times.Never);
            _repository.Verify(x => x.Update(It.IsAny<ItInterface>()), Times.Never);
        }

        [Fact]
        public void Deactivate_Returns_Interface_With_No_Changes_If_Already_Deactivated()
        {
            //Arrange
            var itInterface = CreateInterfaceWithAllBasicPropertiesSet();
            itInterface.Disabled = true;
            var transaction = SetupTransaction();
            _repository.Setup(x => x.GetInterface(itInterface.Id)).Returns(itInterface);
            _authorizationContext.Setup(x => x.AllowModify(itInterface)).Returns(true);

            //Act
            var result = _sut.Deactivate(itInterface.Id);

            //Assert
            Assert.True(result.Ok);
            Assert.True(result.Select(x => x.Disabled).Value);

            _domainEvents.Verify(x => x.Raise(It.IsAny<EnabledStatusChanged<ItInterface>>()), Times.Never);
            transaction.Verify(x => x.Commit(), Times.Never);
            _repository.Verify(x => x.Update(It.IsAny<ItInterface>()), Times.Never);
        }

        [Fact]
        public void Activate_Returns_Ok()
        {
            //Arrange
            var itInterface = CreateInterfaceWithAllBasicPropertiesSet(true);
            var transaction = SetupTransaction();
            _repository.Setup(x => x.GetInterface(itInterface.Id)).Returns(itInterface);
            _authorizationContext.Setup(x => x.AllowModify(itInterface)).Returns(true);

            //Act
            var result = _sut.Activate(itInterface.Id);

            //Assert
            Assert.True(result.Ok);
            Assert.False(result.Select(x => x.Disabled).Value);

            _domainEvents.Verify(x => x.Raise(It.IsAny<EnabledStatusChanged<ItInterface>>()), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
            _repository.Verify(x => x.Update(It.IsAny<ItInterface>()), Times.Once);
        }

        [Fact]
        public void Cannot_Activate_If_No_Access()
        {
            //Arrange
            var itInterface = CreateInterfaceWithAllBasicPropertiesSet(true);
            var transaction = SetupTransaction();
            _repository.Setup(x => x.GetInterface(itInterface.Id)).Returns(itInterface);
            _authorizationContext.Setup(x => x.AllowModify(itInterface)).Returns(false);

            //Act
            var result = _sut.Activate(itInterface.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);

            _domainEvents.Verify(x => x.Raise(It.IsAny<EnabledStatusChanged<ItInterface>>()), Times.Never);
            transaction.Verify(x => x.Commit(), Times.Never);
            _repository.Verify(x => x.Update(It.IsAny<ItInterface>()), Times.Never);
        }

        [Fact]
        public void Cannot_Activate_If_Not_Found()
        {
            //Arrange
            var itInterface = CreateInterfaceWithAllBasicPropertiesSet(true);
            var transaction = SetupTransaction();
            _repository.Setup(x => x.GetInterface(itInterface.Id)).Returns(Maybe<ItInterface>.None);

            //Act
            var result = _sut.Activate(itInterface.Id);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);

            _domainEvents.Verify(x => x.Raise(It.IsAny<EnabledStatusChanged<ItInterface>>()), Times.Never);
            transaction.Verify(x => x.Commit(), Times.Never);
            _repository.Verify(x => x.Update(It.IsAny<ItInterface>()), Times.Never);
        }

        [Fact]
        public void Activate_Returns_Interface_With_No_Changes_If_Already_Activated()
        {
            //Arrange
            var itInterface = CreateInterfaceWithAllBasicPropertiesSet(false);
            var transaction = SetupTransaction();
            _repository.Setup(x => x.GetInterface(itInterface.Id)).Returns(itInterface);
            _authorizationContext.Setup(x => x.AllowModify(itInterface)).Returns(true);

            //Act
            var result = _sut.Activate(itInterface.Id);

            //Assert
            Assert.True(result.Ok);
            Assert.False(result.Select(x => x.Disabled).Value);

            _domainEvents.Verify(x => x.Raise(It.IsAny<EnabledStatusChanged<ItInterface>>()), Times.Never);
            transaction.Verify(x => x.Commit(), Times.Never);
            _repository.Verify(x => x.Update(It.IsAny<ItInterface>()), Times.Never);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public void Can_Get_Permissions(bool read, bool modify, bool delete)
        {
            //Arrange
            var uuid = A<Guid>();
            var itInterface = new ItInterface { Uuid = uuid };
            ExpectGetInterfaceReturns(uuid, itInterface);
            ExpectAllowReadReturns(itInterface, read);
            ExpectAllowModifyReturns(itInterface, modify);
            ExpectAllowDeleteReturns(itInterface, delete);

            //Act
            var result = _sut.GetPermissions(uuid);

            //Assert
            Assert.True(result.Ok);
            var permissions = result.Value;
            Assert.Equivalent(new ItInterfacePermissions(new ResourcePermissionsResult(read, modify, delete), new List<ItInterfaceDeletionConflict>()), permissions);
        }

        [Fact]
        public void Can_Get_Permissions_With_DeletionConflict()
        {
            //Arrange
            var uuid = A<Guid>();
            var itInterface = new ItInterface
            {
                Uuid = uuid,
                ExhibitedBy = new ItInterfaceExhibit()
                {
                    ItSystem = new ItSystem()
                }
            };
            ExpectGetInterfaceReturns(uuid, itInterface);
            ExpectAllowReadReturns(itInterface, true);
            ExpectAllowModifyReturns(itInterface, true);
            ExpectAllowDeleteReturns(itInterface, true);

            //Act
            var result = _sut.GetPermissions(uuid);

            //Assert
            Assert.True(result.Ok);
            var permissions = result.Value;
            var basePermissions = new ResourcePermissionsResult(true, true, true);
            var conflicts = new List<ItInterfaceDeletionConflict>()
            {
                ItInterfaceDeletionConflict.ExposedByItSystem
            };
            var expectedPermissions = new ItInterfacePermissions(basePermissions, conflicts);
            Assert.Equivalent(expectedPermissions, permissions);
        }

        [Fact]
        public void Get_Permissions_Returns_Not_Found()
        {
            //Arrange
            var wrongUuid = A<Guid>();
            ExpectGetInterfaceReturns(wrongUuid, Maybe<ItInterface>.None);

            //Act
            var result = _sut.GetPermissions(wrongUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Get_CollectionPermissions(bool create)
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var organization = new Organization { Id = A<int>() };

            ExpectGetOrganization(organizationUuid, organization);
            ExpectAllowCreate(organization.Id, create);

            //Act
            var result = _sut.GetCollectionPermissions(organizationUuid);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(create, result.Value.Create);
        }

        [Fact]
        public void Get_CollectionPermissions_Returns_OperationError_When_GetOrganization_Fails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var error = A<OperationError>();

            ExpectGetOrganization(organizationUuid, error);

            //Act
            var result = _sut.GetCollectionPermissions(organizationUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(error.FailureType, result.Error.FailureType);
        }

        [Fact]
        public void Stakeholders_Can_Access_All_Interfaces()
        {

            var localInterface = new ItInterface { AccessModifier = AccessModifier.Local };
            var publicInterface = new ItInterface { AccessModifier = AccessModifier.Public };
            var interfaces = new List<ItInterface> { localInterface, publicInterface };
            ExpectInterfaceRepositoryReturns(interfaces);
            ExpectUserOrganizationIdsReturns(new List<int>());
            ExpectUserIsStakeholder();

            var result = _sut.GetAvailableInterfaces();

            Assert.Equal(2, result.Count());

        }

        private void ExpectUserIsStakeholder()
        {
            ExpectCrossOrganizationReadAccessReturns(CrossOrganizationDataReadAccessLevel.Public); //Access level for stakeholders
            ExpectIsStakeHolder(true);
        }

        private void ExpectCrossOrganizationReadAccessReturns(CrossOrganizationDataReadAccessLevel accessLevel)
        {
            _authorizationContext.Setup(x => x.GetCrossOrganizationReadAccess()).Returns(accessLevel);
        }

        private void ExpectIsStakeHolder(bool isStakeHolder)
        {
            _userContext.Setup(x => x.HasStakeHolderAccess()).Returns(isStakeHolder);
        }

        private void ExpectInterfaceRepositoryReturns(IEnumerable<ItInterface> interfaces)
        {
            _repository.Setup(x => x.GetInterfaces()).Returns(interfaces.AsQueryable());
        }

        private void ExpectUserOrganizationIdsReturns(IEnumerable<int> ids)
        {
            _userContext.Setup(x => x.OrganizationIds).Returns(ids);
        }

        private void Test_Command_Which_Mutates_ItInterface_With_Success(
            Func<ItInterface, Result<ItInterface, OperationError>> command)
        {
            //Arrange
            var itInterface = CreateInterfaceWithAllBasicPropertiesSet();
            var transaction = SetupTransaction();
            _repository.Setup(x => x.GetInterface(itInterface.Id)).Returns(itInterface);
            _authorizationContext.Setup(x => x.AllowModify(itInterface)).Returns(true);

            //Act
            var updated = command(itInterface);

            //Assert
            Assert.True(updated.Ok);
            _domainEvents.Verify(x => x.Raise(It.IsAny<EntityUpdatedEvent<ItInterface>>()), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        private void Test_Command_Which_Fails_Mutating_ItInterface(
            Func<ItInterface, Result<ItInterface, OperationError>> command,
            Action<OperationError> verifyError)
        {
            //Arrange
            var itInterface = CreateInterfaceWithAllBasicPropertiesSet();
            var transaction = SetupTransaction();
            _repository.Setup(x => x.GetInterface(itInterface.Id)).Returns(itInterface);
            _authorizationContext.Setup(x => x.AllowModify(itInterface)).Returns(true);

            //Act
            var updated = command(itInterface);

            //Assert
            Assert.True(updated.Failed);
            verifyError(updated.Error);
            _domainEvents.Verify(x => x.Raise(It.IsAny<EntityUpdatedEvent<ItInterface>>()), Times.Never);
            transaction.Verify(x => x.Commit(), Times.Never());
        }

        private void Test_Command_Which_Mutates_ItInterface_With_Failure_Forbidden(
            Func<ItInterface, Result<ItInterface, OperationError>> command)
        {
            //Arrange
            var itInterface = CreateInterfaceWithAllBasicPropertiesSet();
            var transaction = SetupTransaction();
            _repository.Setup(x => x.GetInterface(itInterface.Id)).Returns(itInterface);
            _authorizationContext.Setup(x => x.AllowModify(itInterface)).Returns(false);

            //Act
            var updated = command(itInterface);

            //Assert
            Assert.True(updated.Failed);
            Assert.Equal(OperationFailure.Forbidden, updated.Error.FailureType);
            _domainEvents.Verify(x => x.Raise(It.IsAny<EntityUpdatedEvent<ItInterface>>()), Times.Never());
            transaction.Verify(x => x.Commit(), Times.Never());
        }

        private void Test_Command_Which_Mutates_ItInterface_With_Failure_NotFound(
            Func<ItInterface, Result<ItInterface, OperationError>> command)
        {
            //Arrange
            var itInterface = CreateInterfaceWithAllBasicPropertiesSet();
            var transaction = SetupTransaction();
            _repository.Setup(x => x.GetInterface(itInterface.Id)).Returns(Maybe<ItInterface>.None);

            //Act
            var updated = command(itInterface);

            //Assert
            Assert.True(updated.Failed);
            Assert.Equal(OperationFailure.NotFound, updated.Error.FailureType);
            _domainEvents.Verify(x => x.Raise(It.IsAny<EntityUpdatedEvent<ItInterface>>()), Times.Never);
        }

        private ItInterface CreateInterfaceWithAllBasicPropertiesSet(bool disabled = false)
        {
            var organizationId = A<int>();
            return new ItInterface()
            {
                Id = A<int>(),
                Version = A<string>(),
                AccessModifier = A<AccessModifier>(),
                Description = A<string>(),
                ItInterfaceId = A<string>(),
                Name = A<string>(),
                Note = A<string>(),
                Url = A<string>(),
                Uuid = A<Guid>(),
                OrganizationId = organizationId,
                Disabled = disabled,
                Organization = new Organization()
                {
                    Uuid = A<Guid>(),
                    Id = organizationId
                }
            };
        }

        private void ExpectAllowCreate(int orgId, bool returnValue)
        {
            _authorizationContext.Setup(x => x.AllowCreate<ItInterface>(orgId)).Returns(returnValue);
        }

        private Mock<IDatabaseTransaction> SetupTransaction()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);
            return transaction;
        }

        private void ExpectGetInterfaceReturns(int interfaceId, ItInterface value)
        {
            _repository.Setup(x => x.GetInterface(interfaceId)).Returns(value);
        }

        private void ExpectGetInterfaceReturns(Guid interfaceUuid, Maybe<ItInterface> value)
        {
            _repository.Setup(x => x.GetInterface(interfaceUuid)).Returns(value);
        }

        private void ExpectAllowModifyReturns(ItInterface itInterface, bool value)
        {
            _authorizationContext.Setup(x => x.AllowModify(itInterface)).Returns(value);
        }

        private void ExpectAllowReadReturns(IEntity entity, bool value)
        {
            _authorizationContext.Setup(x => x.AllowReads(entity)).Returns(value);
        }

        private void ExpectGetSystemReturns(int newSystemId, ItSystem value)
        {
            _systemRepository.Setup(x => x.GetSystem(newSystemId)).Returns(value);
        }

        private void ExpectAllowDeleteReturns(ItInterface itInterface, bool value)
        {
            _authorizationContext.Setup(x => x.AllowDelete(itInterface)).Returns(value);
        }

        private void ExpectGetOrganization(Guid orgUuid, Result<Organization, OperationError> result, OrganizationDataReadAccessLevel? accessLevel = null)
        {
            _organizationService.Setup(x => x.GetOrganization(orgUuid, accessLevel)).Returns(result);
        }

        private void ExpectResolveDataTypeOptionReturns(ItInterface itInterface, Guid typeUuid, Result<(DataType option, bool available), OperationError> dataType)
        {
            _optionResolverMock
                .Setup(x => x.GetOptionType<DataRow, DataType>(itInterface.Organization.Uuid, typeUuid)).Returns(dataType);
        }
    }
}
