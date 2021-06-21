using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Interface;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Interface;
using Core.DomainServices.Repositories.Interface;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
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
            _sut = new ItInterfaceService(
                _dataRowRepository.Object,
                _systemRepository.Object,
                _authorizationContext.Object,
                _transactionManager.Object,
                _domainEvents.Object,
                _repository.Object,
                _userContext.Object,
                _operationClock.Object);
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
            _transactionManager.Setup(x => x.Begin(It.IsAny<IsolationLevel>())).Returns(transaction.Object);

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
            var itInterface = new ItInterface() { ExhibitedBy = new ItInterfaceExhibit() { ItSystem = existingSystem } };
            ExpectGetInterfaceReturns(interfaceId, itInterface);
            ExpectAllowModifyReturns(itInterface, true);
            ExpectGetSystemReturns(existingSystem.Id, existingSystem);
            ExpectAllowReadReturns(existingSystem, true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(It.IsAny<IsolationLevel>())).Returns(transaction.Object);

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
            _transactionManager.Setup(x => x.Begin(It.IsAny<IsolationLevel>())).Returns(transaction.Object);

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
        public void Delete_Returns_Ok_And_Raises_Domain_Event()
        {
            //Arrange
            var interfaceId = A<int>();
            var dataRow1 = new DataRow { Id = A<int>() };
            var dataRow2 = new DataRow { Id = A<int>() };
            var interfaceToDelete = new ItInterface
            {
                InterfaceId = interfaceId,
                DataRows = new List<DataRow>()
                {
                    dataRow1,
                    dataRow2
                }
            };
            var transaction = new Mock<IDatabaseTransaction>();
            ExpectGetInterfaceReturns(interfaceId, interfaceToDelete);
            ExpectAllowDeleteReturns(interfaceToDelete, true);
            _transactionManager.Setup(x => x.Begin(IsolationLevel.Serializable)).Returns(transaction.Object);

            //Act
            var result = _sut.Delete(interfaceId);

            //Assert
            Assert.True(result.Ok);
            _domainEvents.Verify(x => x.Raise(It.Is<EntityDeletedEvent<ItInterface>>(d => d.Entity == interfaceToDelete)), Times.Once);
            _dataRowRepository.Verify(x => x.DeleteByKey(dataRow1.Id), Times.Once);
            _dataRowRepository.Verify(x => x.DeleteByKey(dataRow2.Id), Times.Once);
            _dataRowRepository.Verify(x => x.Save(), Times.Once);
            _repository.Verify(x => x.Delete(interfaceToDelete), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
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

        [Fact]
        public void CreateNewItInterface_Returns_Created_Interface_With_User_Specified_Uuid()
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
            _repository.Setup(x => x.GetInterface(uuid)).Returns(Maybe<ItInterface>.None);
            _repository.Setup(x => x.GetInterfaces()).Returns(new List<ItInterface>().AsQueryable()); //Returns nothing so no conflicts exists
            _operationClock.Setup(x => x.Now).Returns(DateTime.Now);

            //Act
            var createdInterfaceResult = _sut.CreateNewItInterface(org.Id, name, itInterfaceId, uuid);

            //Assert
            Assert.True(createdInterfaceResult.Ok);
            Assert.Equal(uuid, createdInterfaceResult.Value.Uuid);
            Assert.Equal(name, createdInterfaceResult.Value.Name);
            Assert.Equal(itInterfaceId, createdInterfaceResult.Value.ItInterfaceId);
            Assert.Equal(AccessModifier.Public, createdInterfaceResult.Value.AccessModifier); // Defaults to Public access modifier


            _domainEvents.Verify(x => x.Raise(It.IsAny<EntityCreatedEvent<ItInterface>>()), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void CreateNewItInterface_Returns_Created_Interface_With_New_Uuid()
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
            _repository.Setup(x => x.GetInterfaces()).Returns(new List<ItInterface>().AsQueryable()); //Returns nothing so no conflicts exists
            _operationClock.Setup(x => x.Now).Returns(DateTime.Now);

            //Act
            var createdInterfaceResult = _sut.CreateNewItInterface(org.Id, name, itInterfaceId);

            //Assert
            Assert.True(createdInterfaceResult.Ok);
            Assert.IsType<Guid>(createdInterfaceResult.Value.Uuid);
            Assert.Equal(name, createdInterfaceResult.Value.Name);
            Assert.Equal(itInterfaceId, createdInterfaceResult.Value.ItInterfaceId);
            Assert.Equal(AccessModifier.Public, createdInterfaceResult.Value.AccessModifier); // Defaults to Public access modifier

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

        [Fact]
        public void UpdateNameAndInterfaceId_Returns_Updated_Interface()
        {
            Test_Command_Which_Mutates_ItInterface_With_Success(itInterface =>
            {
                //Arrange
                var newItInterfaceId = A<string>();
                var newName = A<string>();

                _repository.Setup(x => x.GetInterfaces()).Returns(new List<ItInterface>().AsQueryable()); // Return nothing so no conflicts are found.

                //Act
                var updatedInterface = _sut.UpdateNameAndInterfaceId(itInterface.Id, newName, newItInterfaceId);

                //Assert
                Assert.Equal(newItInterfaceId, updatedInterface.Value.ItInterfaceId);

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
            _domainEvents.Verify(x => x.Raise(It.IsAny<EntityUpdatedEvent<ItInterface>>()), Times.Never);
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

        private ItInterface CreateInterfaceWithAllBasicPropertiesSet()
        {
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
                OrganizationId = A<int>()
            };
        }

        private void ExpectAllowCreate(int orgId, bool returnValue)
        {
            _authorizationContext.Setup(x => x.AllowCreate<ItInterface>(orgId)).Returns(returnValue);
        }

        private Mock<IDatabaseTransaction> SetupTransaction()
        {
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin(IsolationLevel.ReadCommitted)).Returns(transaction.Object);
            return transaction;
        }

        private void ExpectGetInterfaceReturns(int interfaceId, ItInterface value)
        {
            _repository.Setup(x => x.GetInterface(interfaceId)).Returns(value);
        }

        private void ExpectAllowModifyReturns(ItInterface itInterface, bool value)
        {
            _authorizationContext.Setup(x => x.AllowModify(itInterface)).Returns(value);
        }

        private void ExpectAllowReadReturns(ItSystem itSystem, bool value)
        {
            _authorizationContext.Setup(x => x.AllowReads(itSystem)).Returns(value);
        }

        private void ExpectGetSystemReturns(int newSystemId, ItSystem value)
        {
            _systemRepository.Setup(x => x.GetSystem(newSystemId)).Returns(value);
        }

        private void ExpectAllowDeleteReturns(ItInterface itInterface, bool value)
        {
            _authorizationContext.Setup(x => x.AllowDelete(itInterface)).Returns(value);
        }
    }
}
