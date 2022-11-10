using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Infrastructure.Services.DataAccess;
using Tests.Toolkit.Patterns;
using Moq;
using Xunit;
using Core.DomainModel.Events;
using Core.DomainServices;

namespace Tests.Unit.Core.ApplicationServices.Organizations
{
    public class OrganizationUnitServiceTest : WithAutoFixture
    {
        private readonly OrganizationUnitService _sut;

        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IDomainEvents> _domainEvents;
        private readonly Mock<IDatabaseControl> _databaseControl;
        private readonly Mock<IGenericRepository<OrganizationUnit>> _repositoryMock;

        public OrganizationUnitServiceTest()
        {
            _organizationServiceMock = new Mock<IOrganizationService>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            var organizationRightsServiceMock = new Mock<IOrganizationRightsService>();
            var contractServiceMock = new Mock<IItContractService>();
            var usageServiceMock = new Mock<IItSystemUsageService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            _domainEvents = new Mock<IDomainEvents>();
            _databaseControl = new Mock<IDatabaseControl>();
            _repositoryMock = new Mock<IGenericRepository<OrganizationUnit>>();

            _sut = new OrganizationUnitService(
                _organizationServiceMock.Object,
                organizationRightsServiceMock.Object,
                contractServiceMock.Object,
                usageServiceMock.Object,
                _authorizationContextMock.Object,
                _transactionManagerMock.Object,
                _domainEvents.Object,
                _databaseControl.Object,
                _repositoryMock.Object);
        }

        [Fact]
        public void GetOrganizationRegistrations_Returns_NotFound_When_Organization_Was_NotFound()
        {
            var unitUuid = A<Guid>();
            var orgUuid = A<Guid>();
            
            ExpectGetOrganizationReturns(orgUuid, new OperationError(OperationFailure.NotFound), OrganizationDataReadAccessLevel.All);

            var result = _sut.GetRegistrations(orgUuid, unitUuid);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void OrganizationUnitService_Methods_Return_NotFound_When_Organization_Was_NotFound(bool isDeleteSelected, bool isTransferSelected)
        {
            var unitUuid = A<Guid>();
            var orgUuid = A<Guid>();

            ExpectGetOrganizationReturns(orgUuid, new OperationError(OperationFailure.NotFound));
            _transactionManagerMock.Setup(x => x.Begin());

            var maybeResult = Maybe<OperationError>.None;
            if (isDeleteSelected)
            {
                maybeResult = _sut.DeleteRegistrations(orgUuid, unitUuid, CreateEmptyChangeParameters());
            }
            else if(isTransferSelected)
            {
                maybeResult = _sut.TransferRegistrations(orgUuid, unitUuid, A<Guid>(), CreateEmptyChangeParameters());
            }

            Assert.True(maybeResult.HasValue);
            Assert.Equal(OperationFailure.NotFound, maybeResult.Value.FailureType);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void OrganizationUnitService_Methods_Return_Forbidden_When_User_Is_Not_Allowed_To_Modify_Organization(bool isDeleteSelected, bool isTransferSelected)
        {
            var unitUuid = A<Guid>();
            var orgUuid = A<Guid>();
            var org = new Organization { Uuid = orgUuid };
            
            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectAllowModifyReturns(org, false);

            var maybeResult = Maybe<OperationError>.None;
            if (isDeleteSelected)
            {
                maybeResult = _sut.DeleteRegistrations(orgUuid, unitUuid, CreateEmptyChangeParameters());
            }
            else if(isTransferSelected)
            {
                maybeResult = _sut.TransferRegistrations(orgUuid, unitUuid, A<Guid>(), CreateEmptyChangeParameters());
            }

            Assert.True(maybeResult.HasValue);
            Assert.Equal(OperationFailure.Forbidden, maybeResult.Value.FailureType);
            Assert.Equal("User is not allowed to modify the organization", maybeResult.Value.Message);
        }

        [Fact]
        public void GetOrganizationRegistrations_Returns_NotFound()
        {
            var orgUuid = A<Guid>();
            var unitUuid = A<Guid>();
            var operationError = new OperationError(OperationFailure.NotFound);

            ExpectGetOrganizationReturns(orgUuid, new Organization(), OrganizationDataReadAccessLevel.All);
            
            var result = _sut.GetRegistrations(orgUuid, unitUuid);
            Assert.True(result.Failed);
            Assert.Equal(operationError.FailureType, result.Error.FailureType);
        }

        [Fact]
        public void DeleteSelectedOrganizationRegistrations_Returns_NotFound_When_Unit_NotFound()
        {
            var orgUuid = A<Guid>();
            var unitUuid = A<Guid>();
            var org = new Organization()
            {
                Uuid = orgUuid
            };

            ExpectGetOrganizationReturns(orgUuid, org); 
            ExpectAllowModifyReturns(org, true);

            var operationError = new OperationError(OperationFailure.NotFound);
            var result = _sut.DeleteRegistrations(orgUuid, unitUuid, CreateEmptyChangeParameters());

            Assert.True(result.HasValue);
            Assert.Equal(operationError.FailureType, result.Value.FailureType);
        }

        [Fact]
        public void DeleteSelectedOrganizationRegistrations_Returns_Forbidden()
        {
            var orgUuid = A<Guid>();
            var unitUuid = A<Guid>();
            var unit = new OrganizationUnit { Uuid = unitUuid};
            var org = new Organization()
            {
                Uuid = orgUuid,
                OrgUnits = new List<OrganizationUnit>()
                {
                    unit
                }
            };

            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectAllowDeleteReturns(unit, false);

            var result = _sut.DeleteRegistrations(orgUuid, unitUuid, CreateEmptyChangeParameters());
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Forbidden, result.Value.FailureType);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TransferSelectedOrganizationRegistrations_Returns_NotFound(bool isUnitValid)
        {
            var orgUuid = A<Guid>();
            var targetUnitUuid = A<Guid>();
            var unitUuid = A<Guid>();

            var org = new Organization
            {
                Uuid = orgUuid
            };

            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectAllowModifyReturns(org, true);

            //If unit is valid check if target unit returns NotFound
            if (isUnitValid)
            {
                var unit = new OrganizationUnit {Uuid = unitUuid};
                org.OrgUnits = new List<OrganizationUnit> { unit };
                ExpectAllowModifyReturns(unit, true);
                var transaction = new Mock<IDatabaseTransaction>();
                _transactionManagerMock.Setup(x => x.Begin()).Returns(transaction.Object);
            }

            var result = _sut.TransferRegistrations(orgUuid, unitUuid, targetUnitUuid, CreateEmptyChangeParameters());
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.NotFound, result.Value.FailureType);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void TransferSelectedOrganizationRegistrations_Returns_Forbidden(bool isUnitValid, bool isTargetUnitValid)
        {
            var orgUuid = A<Guid>();
            var unitUuid = A<Guid>();
            var targetUnitUuid = A<Guid>();
            var unit = new OrganizationUnit { Uuid = unitUuid };
            var targetUnit = new OrganizationUnit { Uuid = targetUnitUuid };
            var org = new Organization()
            {
                Uuid = orgUuid,
                OrgUnits = new List<OrganizationUnit>
                {
                    unit, targetUnit
                }
            };

            ExpectGetOrganizationReturns(orgUuid, org);
            
            ExpectAllowModifyReturns(unit, isUnitValid);
            ExpectAllowModifyReturns(targetUnit, isTargetUnitValid);

            var result = _sut.TransferRegistrations(orgUuid, unitUuid, targetUnitUuid, CreateEmptyChangeParameters());
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Forbidden, result.Value.FailureType);
        }

        private void ExpectGetOrganizationUnitReturns(Guid uuid, OrganizationUnit result)
        {
            _organizationServiceMock.Setup(x => x.GetOrganizationUnit(uuid)).Returns(result);
        }

        private void ExpectGetOrganizationUnitReturns(Guid uuid, OperationError result)
        {
            _organizationServiceMock.Setup(x => x.GetOrganizationUnit(uuid)).Returns(result);
        }

        private void ExpectGetOrganizationReturns(Guid uuid, Organization result, OrganizationDataReadAccessLevel? readAccessLevel = null)
        {
            _organizationServiceMock.Setup(x => x.GetOrganization(uuid, readAccessLevel)).Returns(result);
        }

        private void ExpectGetOrganizationReturns(Guid uuid, OperationError result, OrganizationDataReadAccessLevel? readAccessLevel = null)
        {
            _organizationServiceMock.Setup(x => x.GetOrganization(uuid, readAccessLevel)).Returns(result);
        }

        private void ExpectAllowReadsReturns(IEntity unit, bool result)
        {
            _authorizationContextMock.Setup(x => x.AllowReads(unit)).Returns(result);
        }

        private void ExpectAllowDeleteReturns(IEntity unit, bool result)
        {
            _authorizationContextMock.Setup(x => x.AllowDelete(unit)).Returns(result);
        }

        private void ExpectAllowModifyReturns(IEntity unit, bool result)
        {
            _authorizationContextMock.Setup(x => x.AllowModify(unit)).Returns(result);
        }

        private static OrganizationUnitRegistrationChangeParameters CreateEmptyChangeParameters()
        {
            return new OrganizationUnitRegistrationChangeParameters(
                new List<int>(), 
                new List<int>(),
                new List<PaymentChangeParameters>(), 
                new List<int>(), 
                new List<int>());
        }
    }
}
