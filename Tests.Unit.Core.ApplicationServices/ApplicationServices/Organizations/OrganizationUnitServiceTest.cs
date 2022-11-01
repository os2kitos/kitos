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
using Core.DomainServices.Generic;
using Core.DomainServices.Authorization;
using Infrastructure.Services.DataAccess;
using Tests.Toolkit.Patterns;
using Moq;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Organizations
{
    public class OrganizationUnitServiceTest : WithAutoFixture
    {
        private readonly OrganizationUnitService _sut;

        private readonly Mock<IEntityIdentityResolver> _identityResolverMock;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<ITransactionManager> _transactionManagerMock;

        public OrganizationUnitServiceTest()
        {
            _identityResolverMock = new Mock<IEntityIdentityResolver>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            var organizationRightsServiceMock = new Mock<IOrganizationRightsService>();
            var contractServiceMock = new Mock<IItContractService>();
            var usageServiceMock = new Mock<IItSystemUsageService>();
            _transactionManagerMock = new Mock<ITransactionManager>();

            _sut = new OrganizationUnitService(
                _identityResolverMock.Object,
                _organizationServiceMock.Object,
                organizationRightsServiceMock.Object,
                contractServiceMock.Object,
                usageServiceMock.Object,
                _authorizationContextMock.Object,
                _transactionManagerMock.Object);
        }

        [Fact]
        public void GetOrganizationRegistrations_Returns_NotFound_When_OrganizationUuid_Was_NotFound()
        {
            var unitId = A<int>();
            var orgId = A<int>();

            ExpectResolveOrganizationUuidReturns(orgId, Maybe<Guid>.None);

            ExpectResolveOrganizationUnitUuidReturns(unitId, Maybe<Guid>.None);
            
            var result = _sut.GetOrganizationRegistrations(orgId, unitId);
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
            Assert.Equal($"Organization with id: {orgId} not found", result.Error.Message);
        }

        [Fact]
        public void GetOrganizationRegistrations_Returns_NotFound_When_Organization_Was_NotFound()
        {
            var unitId = A<int>();
            var orgId = A<int>();
            var orgUuid = A<Guid>();
            var org = new Organization { Id = orgId, Uuid = orgUuid };

            ExpectResolveOrganizationUuidReturns(orgId, Maybe<Guid>.None);
            ExpectGetOrganizationReturns(orgUuid, org, OrganizationDataReadAccessLevel.All);

            ExpectResolveOrganizationUnitUuidReturns(unitId, Maybe<Guid>.None);

            var result = _sut.GetOrganizationRegistrations(orgId, unitId);
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
            Assert.Equal($"Organization with id: {orgId} not found", result.Error.Message);
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        public void OrganizationUnitService_Methods_Return_NotFound_When_OrganizationUuid_Was_NotFound(bool isDeleteSelected, bool isDeleteAll, bool isTransferSelected)
        {
            var unitId = A<int>();
            var orgId = A<int>();

            ExpectResolveOrganizationUuidReturns(orgId, Maybe<Guid>.None);

            ExpectResolveOrganizationUnitUuidReturns(unitId, Maybe<Guid>.None);

            var maybeResult = Maybe<OperationError>.None;
            if (isDeleteSelected)
            {
                maybeResult = _sut.DeleteSelectedOrganizationRegistrations(orgId, unitId, CreateEmptyChangeParameters());
            }
            else if (isDeleteAll)
            {
                maybeResult = _sut.DeleteAllUnitOrganizationRegistrations(orgId, unitId);
            }
            else if(isTransferSelected)
            {
                maybeResult = _sut.TransferSelectedOrganizationRegistrations(orgId, unitId, A<int>(), CreateEmptyChangeParameters());
            }

            Assert.True(maybeResult.HasValue);
            Assert.Equal(OperationFailure.NotFound, maybeResult.Value.FailureType);
            Assert.Equal($"Organization with id: {orgId} not found", maybeResult.Value.Message);
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        public void OrganizationUnitService_Methods_Return_NotFound_When_Organization_Was_NotFound(bool isDeleteSelected, bool isDeleteAll, bool isTransferSelected)
        {
            var unitId = A<int>();
            var orgId = A<int>();
            var orgUuid = A<Guid>();

            ExpectResolveOrganizationUuidReturns(orgId, orgUuid);
            ExpectGetOrganizationReturns(orgUuid, new OperationError(OperationFailure.NotFound));

            ExpectResolveOrganizationUnitUuidReturns(unitId, Maybe<Guid>.None);

            var maybeResult = Maybe<OperationError>.None;
            if (isDeleteSelected)
            {
                maybeResult = _sut.DeleteSelectedOrganizationRegistrations(orgId, unitId, CreateEmptyChangeParameters());
            }
            else if (isDeleteAll)
            {
                maybeResult = _sut.DeleteAllUnitOrganizationRegistrations(orgId, unitId);
            }
            else if(isTransferSelected)
            {
                maybeResult = _sut.TransferSelectedOrganizationRegistrations(orgId, unitId, A<int>(), CreateEmptyChangeParameters());
            }

            Assert.True(maybeResult.HasValue);
            Assert.Equal(OperationFailure.NotFound, maybeResult.Value.FailureType);
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        public void OrganizationUnitService_Methods_Return_Forbidden_When_User_Is_Not_Allowed_To_Modify_Organization(bool isDeleteSelected, bool isDeleteAll, bool isTransferSelected)
        {
            var unitId = A<int>();
            var orgId = A<int>();
            var orgUuid = A<Guid>();
            var org = new Organization { Id = orgId, Uuid = orgUuid };

            ExpectResolveOrganizationUuidReturns(orgId, orgUuid);
            ExpectGetOrganizationReturns(orgUuid, org);
            ExpectAllowModifyReturns(org, false);

            var maybeResult = Maybe<OperationError>.None;
            if (isDeleteSelected)
            {
                maybeResult = _sut.DeleteSelectedOrganizationRegistrations(orgId, unitId, CreateEmptyChangeParameters());
            }
            else if (isDeleteAll)
            {
                maybeResult = _sut.DeleteAllUnitOrganizationRegistrations(orgId, unitId);
            }
            else if(isTransferSelected)
            {
                maybeResult = _sut.TransferSelectedOrganizationRegistrations(orgId, unitId, A<int>(), CreateEmptyChangeParameters());
            }

            Assert.True(maybeResult.HasValue);
            Assert.Equal(OperationFailure.Forbidden, maybeResult.Value.FailureType);
            Assert.Equal("User is not allowed to modify the organization", maybeResult.Value.Message);
        }

        [Fact]
        public void GetOrganizationRegistrations_Returns_NotFound_When_Uuid_Is_NotFound()
        {
            var unitId = A<int>();
            var orgId = A<int>();

            ExpectGetOrganizationReturnsTrue(orgId, OrganizationDataReadAccessLevel.All);

            ExpectResolveOrganizationUnitUuidReturns(unitId, Maybe<Guid>.None);

            var result = _sut.GetOrganizationRegistrations(orgId, unitId);
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
            Assert.Equal($"Organization unit with id: {unitId} not found", result.Error.Message);
        }

        [Fact]
        public void GetOrganizationRegistrations_Returns_NotFound()
        {
            var unitId = A<int>();
            var orgId = A<int>();
            var unitUuid = A<Guid>();
            var operationError = new OperationError(OperationFailure.NotFound);

            ExpectGetOrganizationReturnsTrue(orgId, OrganizationDataReadAccessLevel.All);

            ExpectResolveOrganizationUnitUuidReturns(unitId, unitUuid);
            ExpectGetOrganizationUnitReturns(unitUuid, operationError);

            var result = _sut.GetOrganizationRegistrations(orgId, unitId);
            Assert.True(result.Failed);
            Assert.Equal(operationError.FailureType, result.Error.FailureType);
        }

        [Fact]
        public void GetOrganizationRegistrations_Returns_Forbidden()
        {
            var unitId = A<int>();
            var orgId = A<int>();
            var unitUuid = A<Guid>();
            var unit = new OrganizationUnit() {Id = unitId, Uuid = unitUuid};

            ExpectGetOrganizationReturnsTrue(orgId, OrganizationDataReadAccessLevel.All);

            ExpectResolveOrganizationUnitUuidReturns(unitId, unitUuid);
            ExpectGetOrganizationUnitReturns(unitUuid, unit);
            ExpectAllowReadsReturns(unit, false);

            var result = _sut.GetOrganizationRegistrations(orgId, unitId);
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void DeleteSelectedOrganizationRegistrations_Returns_NotFound_When_Uuid_NotFound()
        {
            var unitId = A<int>();
            var orgId = A<int>();
            
            ExpectGetOrganizationReturnsTrue(orgId);
            ExpectResolveOrganizationUnitUuidReturns(unitId, Maybe<Guid>.None);

            var result = _sut.DeleteSelectedOrganizationRegistrations(orgId, unitId, CreateEmptyChangeParameters());
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.NotFound, result.Value.FailureType);
        }

        [Fact]
        public void DeleteSelectedOrganizationRegistrations_Returns_NotFound_When_Unit_NotFound()
        {
            var unitId = A<int>();
            var unitUuid = A<Guid>();
            var orgId = A<int>();

            ExpectGetOrganizationReturnsTrue(orgId);
            ExpectResolveOrganizationUnitUuidReturns(unitId, unitUuid);
            ExpectGetOrganizationUnitReturns(unitUuid, new OperationError(OperationFailure.NotFound));

            var operationError = new OperationError(OperationFailure.NotFound);
            var result = _sut.DeleteSelectedOrganizationRegistrations(orgId, unitId, CreateEmptyChangeParameters());

            Assert.True(result.HasValue);
            Assert.Equal(operationError.FailureType, result.Value.FailureType);
            Assert.Equal(operationError.Message, result.Value.Message);
        }

        [Fact]
        public void DeleteSelectedOrganizationRegistrations_Returns_Forbidden()
        {
            var unitId = A<int>();
            var orgId = A<int>();
            var unitUuid = A<Guid>();
            var unit = new OrganizationUnit() {Id = unitId, Uuid = unitUuid};

            ExpectGetOrganizationReturnsTrue(orgId);
            ExpectResolveOrganizationUnitUuidReturns(unitId, unitUuid);
            ExpectGetOrganizationUnitReturns(unitUuid, unit);
            ExpectAllowDeleteReturns(unit, false);

            var result = _sut.DeleteSelectedOrganizationRegistrations(orgId, unitId, CreateEmptyChangeParameters());
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Forbidden, result.Value.FailureType);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void TransferSelectedOrganizationRegistrations_Returns_NotFound_When_Uuid_Is_NotFound(bool isUnitValid, bool isTargetUnitValid)
        {
            var unitId = A<int>();
            var orgId = A<int>();
            var unitUuid = isUnitValid ? A<Guid>() : Maybe<Guid>.None;
            var targetUnitId = A<int>();

            ExpectGetOrganizationReturnsTrue(orgId);
            ExpectResolveOrganizationUnitUuidReturns(unitId, unitUuid);
            ExpectResolveOrganizationUnitUuidReturns(targetUnitId, isTargetUnitValid ? A<Guid>() : Maybe<Guid>.None);
            if(isUnitValid)
                ExpectGetOrganizationUnitReturns(unitUuid.Value, new OrganizationUnit());

            var result = _sut.TransferSelectedOrganizationRegistrations(orgId, unitId, targetUnitId, CreateEmptyChangeParameters());
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.NotFound, result.Value.FailureType);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void TransferSelectedOrganizationRegistrations_Returns_NotFound(bool isUnitValid, bool isTargetUnitValid)
        {
            var unitId = A<int>();
            var orgId = A<int>();
            var targetUnitId = A<int>();
            var unitUuid = A<Guid>();
            var targetUnitIdUuid = A<Guid>();
            var expectedMessage = "Organization not found";
            var operationError = new OperationError(expectedMessage, OperationFailure.NotFound);

            ExpectGetOrganizationReturnsTrue(orgId);

            ExpectResolveOrganizationUnitUuidReturns(unitId, unitUuid);
            ExpectResolveOrganizationUnitUuidReturns(targetUnitId, targetUnitIdUuid);
            if (isUnitValid)
            {
                ExpectGetOrganizationUnitReturns(unitUuid, new OrganizationUnit());
                ExpectGetOrganizationUnitReturns(targetUnitIdUuid, operationError);
            }
            else if(isTargetUnitValid)
            {
                ExpectGetOrganizationUnitReturns(unitUuid, operationError);
                ExpectGetOrganizationUnitReturns(targetUnitIdUuid, new OrganizationUnit());
            }
            else
            {
                throw new Exception("Invalid data");
            }
            
            var result = _sut.TransferSelectedOrganizationRegistrations(orgId, unitId, targetUnitId, CreateEmptyChangeParameters());
            Assert.True(result.HasValue);
            Assert.Equal(operationError.FailureType, result.Value.FailureType);
            Assert.Equal(operationError.Message, result.Value.Message);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void TransferSelectedOrganizationRegistrations_Returns_Forbidden(bool isUnitValid, bool isTargetUnitValid)
        {
            var unitId = A<int>();
            var orgId = A<int>();
            var targetUnitId = A<int>();
            var unitUuid = A<Guid>();
            var targetUnitUuid = A<Guid>();
            var unit = new OrganizationUnit { Id = unitId, Uuid = unitUuid };
            var targetUnit = new OrganizationUnit { Id = targetUnitId, Uuid = targetUnitUuid };

            ExpectGetOrganizationReturnsTrue(orgId);

            ExpectResolveOrganizationUnitUuidReturns(unitId, unitUuid);
            ExpectResolveOrganizationUnitUuidReturns(targetUnitId, targetUnitUuid);

            ExpectGetOrganizationUnitReturns(unitUuid, unit);
            ExpectGetOrganizationUnitReturns(targetUnitUuid, targetUnit);

            ExpectAllowModifyReturns(unit, isUnitValid);
            ExpectAllowModifyReturns(targetUnit, isTargetUnitValid);

            var result = _sut.TransferSelectedOrganizationRegistrations(orgId, unitId, targetUnitId, CreateEmptyChangeParameters());
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Forbidden, result.Value.FailureType);
        }

        private void ExpectGetOrganizationReturnsTrue(int orgId, OrganizationDataReadAccessLevel? readAccessLevel = null)
        {
            var orgUuid = A<Guid>();
            var org = new Organization { Id = orgId, Uuid = orgUuid };

            ExpectResolveOrganizationUuidReturns(orgId, orgUuid);
            ExpectGetOrganizationReturns(orgUuid, org, readAccessLevel);
            ExpectAllowModifyReturns(org, true);
        }

        private void ExpectResolveOrganizationUnitUuidReturns(int id, Maybe<Guid> result)
        {
            _identityResolverMock.Setup(x => x.ResolveUuid<OrganizationUnit>(id)).Returns(result);
        }

        private void ExpectResolveOrganizationUuidReturns(int id, Maybe<Guid> result)
        {
            _identityResolverMock.Setup(x => x.ResolveUuid<Organization>(id)).Returns(result);
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

        private static OrganizationRegistrationChangeParameters CreateEmptyChangeParameters()
        {
            return new OrganizationRegistrationChangeParameters(
                new List<int>(), 
                new List<int>(),
                new List<PaymentChangeParameters>(), 
                new List<int>(), 
                new List<int>());
        }
    }
}
