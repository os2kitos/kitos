using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainModel.Organization.DomainEvents;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class OrganizationRightsServiceTest : WithAutoFixture
    {
        private readonly OrganizationRightsService _sut;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IGenericRepository<OrganizationRight>> _organizationRightRepository;
        private readonly Mock<IGenericRepository<OrganizationUnitRight>> _organizationUnitRightRepository;
        private readonly Mock<IOrganizationalUserContext> _organizationUserContext;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IGenericRepository<Organization>> _orgRepository;
        private readonly Mock<IDomainEvents> _domainEvents;

        public OrganizationRightsServiceTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _organizationRightRepository = new Mock<IGenericRepository<OrganizationRight>>();
            _organizationUserContext = new Mock<IOrganizationalUserContext>();
            _organizationUnitRightRepository = new Mock<IGenericRepository<OrganizationUnitRight>>();
            _transactionManager = new Mock<ITransactionManager>(); 
            _orgRepository = new Mock<IGenericRepository<Organization>>();
            _domainEvents = new Mock<IDomainEvents>();

            _sut = new OrganizationRightsService(_authorizationContext.Object,
                _organizationRightRepository.Object,
                _organizationUserContext.Object,
                _domainEvents.Object,
                Mock.Of<ILogger>(), 
                _organizationUnitRightRepository.Object,
                _transactionManager.Object,
                _orgRepository.Object);
        }

        [Fact]
        public void RemoveRole_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            _organizationRightRepository.Setup(x => x.GetByKey(id)).Returns(default(OrganizationRight));

            //Act
            var result = _sut.RemoveRole(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void RemoveRole_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var organizationRight = new OrganizationRight();
            _organizationRightRepository.Setup(x => x.GetByKey(id)).Returns(organizationRight);
            _authorizationContext.Setup(x => x.AllowDelete(organizationRight)).Returns(false);

            //Act
            var result = _sut.RemoveRole(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void RemoveRole_Returns_Conflict_If_Global_Admin_Tries_To_Remove_Self()
        {
            //Arrange
            var id = A<int>();
            int usrId = A<int>();
            var organizationRight = new OrganizationRight()
            {
                UserId = usrId,
                Role = OrganizationRole.GlobalAdmin
            };
            _organizationRightRepository.Setup(x => x.GetByKey(id)).Returns(organizationRight);
            _authorizationContext.Setup(x => x.AllowDelete(organizationRight)).Returns(true);
            _organizationUserContext.Setup(x => x.UserId).Returns(usrId);

            //Act
            var result = _sut.RemoveRole(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Conflict, result.Error);
        }

        [Fact]
        public void RemoveRole_Returns_Ok()
        {
            //Arrange
            var id = A<int>();
            var organizationRight = new OrganizationRight()
            {
                UserId = A<int>(),
                Id = id
            };
            _organizationRightRepository.Setup(x => x.GetByKey(id)).Returns(organizationRight);
            _authorizationContext.Setup(x => x.AllowDelete(organizationRight)).Returns(true);
            _organizationUserContext.Setup(x => x.UserId).Returns(A<int>());

            //Act
            var result = _sut.RemoveRole(id);

            //Assert
            Assert.True(result.Ok);
            _organizationRightRepository.Verify(x => x.DeleteByKey(id), Times.Once);
            _organizationRightRepository.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void AddRightToUser_Returns_Forbidden()
        {
            //Arrange
            var organizationId = A<int>();
            _authorizationContext.Setup(x => x.AllowCreate<OrganizationRight>(organizationId, It.IsAny<OrganizationRight>())).Returns(false);

            //Act
            var result = _sut.AssignRole(organizationId, A<int>(), A<OrganizationRole>());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void AddRightToUser_Returns_Ok()
        {
            //Arrange
            var organizationId = A<int>();
            var userId = A<int>();
            var organizationRole = A<OrganizationRole>();
            _organizationUserContext.Setup(x => x.UserId).Returns(userId);
            _authorizationContext.Setup(x => x.AllowAdministerOrganizationRight(It.IsAny<OrganizationRight>())).Returns(true);
            _organizationRightRepository.Setup(x => x.Insert(It.IsAny<OrganizationRight>())).Returns<OrganizationRight>(right => right);

            //Act
            var result = _sut.AssignRole(organizationId, userId, organizationRole);

            //Assert
            Assert.True(result.Ok);
            var resultValue = result.Value;
            Assert.Equal(organizationId, resultValue.OrganizationId);
            Assert.Equal(userId, resultValue.UserId);
            Assert.Equal(organizationRole, resultValue.Role);
            _organizationRightRepository.Verify(x => x.Insert(It.IsAny<OrganizationRight>()), Times.Once);
            _organizationRightRepository.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void RemoveUnitRightsByIds_Returns_Ok()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var unitId = A<int>();
            var unitUuid = A<Guid>();
            var rightId = A<int>();

            var unit = CreateOrganizationUnit(unitId, unitUuid);
            unit.Rights = new List<OrganizationUnitRight> {CreateUnitRight(unitId, rightId) };

            var organization = CreateOrganization(organizationUuid);
            organization.OrgUnits = new List<OrganizationUnit> {unit};

            ExpectGetOrganizationReturns(organization);
            ExpectAllowModifyReturns(organization, result: true);
            ExpectAllowModifyReturns(unit, result: true);

            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            var error = _sut.RemoveUnitRightsByIds(organizationUuid, unitUuid, new List<int> {rightId});

            Assert.False(error.HasValue);

            transaction.Verify(x => x.Commit(), Times.Once());
            _organizationUnitRightRepository.Verify(x => x.Save(), Times.Once());
            _domainEvents.Verify(x => x.Raise(It.IsAny<AdministrativeAccessRightsChanged>()), Times.Once());
        }

        [Fact]
        public void RemoveUnitRightsByIds_Returns_Organization_NotFound()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var unitUuid = A<Guid>();
            var rightId = A<int>();

            ExpectGetOrganizationReturns(new Organization());

            var error = _sut.RemoveUnitRightsByIds(organizationUuid, unitUuid, new List<int> {rightId});

            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
        }

        [Fact]
        public void RemoveUnitRightsByIds_Returns_Organization_Modify_Forbidden()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var unitUuid = A<Guid>();
            var rightId = A<int>();

            var organization = CreateOrganization(organizationUuid);

            ExpectGetOrganizationReturns(organization);
            ExpectAllowModifyReturns(organization, result: false);

            var error = _sut.RemoveUnitRightsByIds(organizationUuid, unitUuid, new List<int> {rightId});

            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.Forbidden, error.Value.FailureType);
        }


        [Fact]
        public void RemoveUnitRightsByIds_Returns_OrganizationUnit_NotFound()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var unitUuid = A<Guid>();
            var rightId = A<int>();

            var organization = CreateOrganization(organizationUuid);

            ExpectGetOrganizationReturns(organization);
            ExpectAllowModifyReturns(organization, result: true);

            var error = _sut.RemoveUnitRightsByIds(organizationUuid, unitUuid, new List<int> {rightId});

            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
        }

        [Fact]
        public void RemoveUnitRightsByIds_Returns_OrganizationUnit_Modify_Forbidden()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var unitId = A<int>();
            var unitUuid = A<Guid>();
            var rightId = A<int>();

            var organization = CreateOrganization(organizationUuid);
            var unit = CreateOrganizationUnit(unitId, unitUuid);

            organization.OrgUnits = new List<OrganizationUnit> {unit};
            
            ExpectGetOrganizationReturns(organization);
            ExpectAllowModifyReturns(organization, result: true);
            ExpectAllowModifyReturns(unit, result: false);
            
            var error = _sut.RemoveUnitRightsByIds(organizationUuid, unitUuid, new List<int> {rightId});

            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.Forbidden, error.Value.FailureType);
        }

        [Fact]
        public void TransferUnitRightsByIds_Returns_Ok()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var unitId = A<int>();
            var targetId = A<int>();
            var unitUuid = A<Guid>();
            var targetUuid = A<Guid>();
            var rightId = A<int>();

            var unit = CreateOrganizationUnit(unitId, unitUuid);
            var targetUnit = CreateOrganizationUnit(targetId, targetUuid);
            unit.Rights = new List<OrganizationUnitRight> { CreateUnitRight(unitId, rightId) };

            var organization = CreateOrganization(organizationUuid);
            organization.OrgUnits = new List<OrganizationUnit> { unit, targetUnit };

            ExpectGetOrganizationReturns(organization);
            ExpectAllowModifyReturns(organization, result: true);
            ExpectAllowModifyReturns(unit, result: true);
            ExpectAllowModifyReturns(targetUnit, result: true);

            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            var error = _sut.TransferUnitRightsByIds(organizationUuid, unitUuid, targetUuid, new List<int> { rightId });

            Assert.False(error.HasValue);

            _domainEvents.Verify(x => x.Raise(It.IsAny<AdministrativeAccessRightsChanged>()), Times.Once());
            _organizationUnitRightRepository.Verify(x => x.Save(), Times.Once());
            transaction.Verify(x => x.Commit(), Times.Once());
        }

        [Fact]
        public void TransferUnitRightsByIds_Returns_Ok_When_TargetUnit_Has_Same_Role_With_Same_User()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var unitId = A<int>();
            var targetId = A<int>();
            var unitUuid = A<Guid>();
            var targetUuid = A<Guid>();
            var rightId = A<int>();
            var roleId = A<int>();
            var userId = A<int>();

            var unit = CreateOrganizationUnit(unitId, unitUuid);
            var targetUnit = CreateOrganizationUnit(targetId, targetUuid);
            unit.Rights = new List<OrganizationUnitRight> { CreateUnitRight(unitId, rightId, userId, roleId) };
            targetUnit.Rights = new List<OrganizationUnitRight> { CreateUnitRight(unitId, rightId, userId, roleId) };

            var organization = CreateOrganization(organizationUuid);
            organization.OrgUnits = new List<OrganizationUnit> { unit, targetUnit };

            ExpectGetOrganizationReturns(organization);
            ExpectAllowModifyReturns(organization, result: true);
            ExpectAllowModifyReturns(unit, result: true);
            ExpectAllowModifyReturns(targetUnit, result: true);

            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            var error = _sut.TransferUnitRightsByIds(organizationUuid, unitUuid, targetUuid, new List<int> { rightId });

            Assert.False(error.HasValue);

            _domainEvents.Verify(x => x.Raise(It.IsAny<AdministrativeAccessRightsChanged>()), Times.Once());
            _organizationUnitRightRepository.Verify(x => x.Save(), Times.Once());
            transaction.Verify(x => x.Commit(), Times.Once());
        }

        [Fact]
        public void TransferUnitRightsByIds_Returns_Organization_NotFound()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var unitUuid = A<Guid>();
            var targetUuid = A<Guid>();
            var rightId = A<int>();

            ExpectGetOrganizationReturns(new Organization());

            var error = _sut.TransferUnitRightsByIds(organizationUuid, unitUuid, targetUuid, new List<int> { rightId });

            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
        }

        [Fact]
        public void TransferUnitRightsByIds_Returns_Organization_Modify_Forbidden()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var unitUuid = A<Guid>();
            var targetUuid = A<Guid>();
            var rightId = A<int>();

            var organization = CreateOrganization(organizationUuid);

            ExpectGetOrganizationReturns(organization);
            ExpectAllowModifyReturns(organization, result: false);

            var error = _sut.TransferUnitRightsByIds(organizationUuid, unitUuid, targetUuid, new List<int> { rightId });

            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.Forbidden, error.Value.FailureType);
        }

        [Fact]
        public void TransferUnitRightsByIds_Returns_OrganizationUnit_NotFound()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var unitUuid = A<Guid>();
            var targetUuid = A<Guid>();
            var rightId = A<int>();

            var organization = CreateOrganization(organizationUuid);

            ExpectGetOrganizationReturns(organization);
            ExpectAllowModifyReturns(organization, result: true);

            var error = _sut.TransferUnitRightsByIds(organizationUuid, unitUuid, targetUuid, new List<int> { rightId });

            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
        }

        [Fact]
        public void TransferUnitRightsByIds_Returns_OrganizationUnit_Modify_Forbidden()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var unitId = A<int>();
            var unitUuid = A<Guid>();
            var targetUuid = A<Guid>();
            var rightId = A<int>();

            var unit = CreateOrganizationUnit(unitId, unitUuid);

            var organization = CreateOrganization(organizationUuid);
            organization.OrgUnits = new List<OrganizationUnit> { unit};

            ExpectGetOrganizationReturns(organization);
            ExpectAllowModifyReturns(organization, result: true);

            var error = _sut.TransferUnitRightsByIds(organizationUuid, unitUuid, targetUuid, new List<int> { rightId });

            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.Forbidden, error.Value.FailureType);
        }

        [Fact]
        public void TransferUnitRightsByIds_Returns_TargetOrganizationUnit_NotFound()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var unitId = A<int>();
            var unitUuid = A<Guid>();
            var targetUuid = A<Guid>();
            var rightId = A<int>();

            var unit = CreateOrganizationUnit(unitId, unitUuid);

            var organization = CreateOrganization(organizationUuid);
            organization.OrgUnits = new List<OrganizationUnit> { unit };

            ExpectGetOrganizationReturns(organization);
            ExpectAllowModifyReturns(organization, result: true);
            ExpectAllowModifyReturns(unit, result: true);

            var error = _sut.TransferUnitRightsByIds(organizationUuid, unitUuid, targetUuid, new List<int> { rightId });

            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
        }

        [Fact]
        public void TransferUnitRightsByIds_Returns_TargetOrganizationUnit_Modify_Forbidden()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var unitId = A<int>();
            var targetId = A<int>();
            var unitUuid = A<Guid>();
            var targetUuid = A<Guid>();
            var rightId = A<int>();

            var unit = CreateOrganizationUnit(unitId, unitUuid);
            var targetUnit = CreateOrganizationUnit(targetId, targetUuid);

            var organization = CreateOrganization(organizationUuid);
            organization.OrgUnits = new List<OrganizationUnit> { unit, targetUnit };

            ExpectGetOrganizationReturns(organization);
            ExpectAllowModifyReturns(organization, result: true);
            ExpectAllowModifyReturns(unit, result: true);
            ExpectAllowModifyReturns(targetUnit, result: false);

            var error = _sut.TransferUnitRightsByIds(organizationUuid, unitUuid, targetUuid, new List<int> { rightId });

            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.Forbidden, error.Value.FailureType);
        }

        [Fact]
        public void TransferUnitRightsByIds_Returns_OrganizationUnitRight_NotFound()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var unitId = A<int>();
            var targetId = A<int>();
            var unitUuid = A<Guid>();
            var targetUuid = A<Guid>();
            var rightId = A<int>();

            var unit = CreateOrganizationUnit(unitId, unitUuid);
            var targetUnit = CreateOrganizationUnit(targetId, targetUuid);

            var organization = CreateOrganization(organizationUuid);
            organization.OrgUnits = new List<OrganizationUnit> { unit, targetUnit };

            ExpectGetOrganizationReturns(organization);
            ExpectAllowModifyReturns(organization, result: true);
            ExpectAllowModifyReturns(unit, result: true);
            ExpectAllowModifyReturns(targetUnit, result: true);

            var error = _sut.TransferUnitRightsByIds(organizationUuid, unitUuid, targetUuid, new List<int> { rightId });

            Assert.True(error.HasValue);
            Assert.Equal(OperationFailure.NotFound, error.Value.FailureType);
        }

        private OrganizationUnit CreateOrganizationUnit(int id, Guid uuid)
        {
            return new OrganizationUnit { Id = id, Uuid = uuid };
        }

        private OrganizationUnitRight CreateUnitRight(int unitId, int rightId, int? userId = null, int? roleId = null)
        {
            var localUserId = userId ?? A<int>();
            var localRoleId = roleId ?? A<int>();

            return new OrganizationUnitRight
            {
                Id = rightId,
                UserId = localUserId,
                User = new User { Id = localUserId },
                ObjectId = unitId,
                Role = new OrganizationUnitRole { Id = localRoleId },
                RoleId = localRoleId
            };
        }

        private Organization CreateOrganization(Guid uuid)
        {
            return new Organization
            {
                Uuid = uuid
            };
        }

        private void ExpectGetOrganizationReturns(Organization organization)
        {
            _orgRepository.Setup(x => x.AsQueryable()).Returns(new List<Organization>{ organization }.AsQueryable());
        }

        private void ExpectAllowModifyReturns<T>(T entity, bool result) where T : IEntity
        {
            _authorizationContext.Setup(x => x.AllowModify(entity)).Returns(result);
        }
    }
}
