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
using Core.DomainServices;
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
            var orgUnitServiceMock = new Mock<IOrgUnitService>();
            _transactionManagerMock = new Mock<ITransactionManager>();
            
            _sut = new OrganizationUnitService(
                _identityResolverMock.Object,
                _organizationServiceMock.Object,
                organizationRightsServiceMock.Object,
                contractServiceMock.Object,
                usageServiceMock.Object,
                _authorizationContextMock.Object,
                orgUnitServiceMock.Object,
                _transactionManagerMock.Object);
        }
        
        [Fact]
        public void GetOrganizationRegistrations_Returns_BadInput()
        {
            var unitId = A<int>();
            var orgId = A<int>();

            ExpectResolveUuidReturns(unitId, Maybe<Guid>.None);

            var result = _sut.GetOrganizationRegistrations(orgId, unitId);
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.Equal("Organization id is invalid", result.Error.Message);
        }

        [Fact]
        public void GetOrganizationRegistrations_Returns_NotFound()
        {
            var unitId = A<int>();
            var orgId = A<int>();
            var unitUuid = A<Guid>();
            var operationError = new OperationError("Organization not found", OperationFailure.NotFound);

            ExpectResolveUuidReturns(unitId, unitUuid);
            ExpectGetOrganizationUnitReturns(unitUuid, operationError);

            var result = _sut.GetOrganizationRegistrations(orgId, unitId);
            Assert.True(result.Failed);
            Assert.Equal(operationError.FailureType, result.Error.FailureType);
            Assert.Equal(operationError.Message, result.Error.Message);
        }

        [Fact]
        public void GetOrganizationRegistrations_Returns_Forbidden()
        {
            var unitId = A<int>();
            var orgId = A<int>();
            var unitUuid = A<Guid>();
            var unit = new OrganizationUnit() {Id = unitId, Uuid = unitUuid};

            ExpectResolveUuidReturns(unitId, unitUuid);
            ExpectGetOrganizationUnitReturns(unitUuid, unit);
            ExpectAllowReadsReturns(unit, false);

            var result = _sut.GetOrganizationRegistrations(orgId, unitId);
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void DeleteSelectedOrganizationRegistrations_Returns_BadInput()
        {
            var unitId = A<int>();
            var orgId = A<int>();

            ExpectResolveUuidReturns(unitId, Maybe<Guid>.None);

            var result = _sut.DeleteSelectedOrganizationRegistrations(orgId, unitId, CreateEmptyChangeParameters());
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
            Assert.Equal("Organization id is invalid", result.Value.Message);
        }

        [Fact]
        public void DeleteSelectedOrganizationRegistrations_Returns_NotFound()
        {
            var unitId = A<int>();
            var orgId = A<int>();
            var unitUuid = A<Guid>();
            var operationError = new OperationError("Organization not found", OperationFailure.NotFound);

            ExpectResolveUuidReturns(unitId, unitUuid);
            ExpectGetOrganizationUnitReturns(unitUuid, operationError);

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

            ExpectResolveUuidReturns(unitId, unitUuid);
            ExpectGetOrganizationUnitReturns(unitUuid, unit);
            ExpectAllowDeleteReturns(unit, false);

            var result = _sut.DeleteSelectedOrganizationRegistrations(orgId, unitId, CreateEmptyChangeParameters());
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Forbidden, result.Value.FailureType);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void TransferSelectedOrganizationRegistrations_Returns_BadInput(bool isUnitValid, bool isTargetUnitValid)
        {
            var unitId = A<int>();
            var orgId = A<int>();
            var unitUuid = isUnitValid ? A<Guid>() : Maybe<Guid>.None;
            var targetUnitId = A<int>();
            const string expectedMessage = "Organization id is invalid";

            ExpectResolveUuidReturns(unitId, unitUuid);
            ExpectResolveUuidReturns(targetUnitId, isTargetUnitValid ? A<Guid>() : Maybe<Guid>.None);
            if(isUnitValid)
                ExpectGetOrganizationUnitReturns(unitUuid.Value, new OrganizationUnit());

            var result = _sut.TransferSelectedOrganizationRegistrations(orgId, unitId, targetUnitId, CreateEmptyChangeParameters());
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
            Assert.Equal(expectedMessage, result.Value.Message);
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

            ExpectResolveUuidReturns(unitId, unitUuid);
            ExpectResolveUuidReturns(targetUnitId, targetUnitIdUuid);
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
            var targetUnitIdUuid = A<Guid>();
            var unit = new OrganizationUnit() { Id = unitId, Uuid = unitUuid };
            var targetUnit = new OrganizationUnit() { Id = targetUnitId, Uuid = targetUnitIdUuid };

            ExpectResolveUuidReturns(unitId, unitUuid);
            ExpectResolveUuidReturns(targetUnitId, targetUnitIdUuid);
            ExpectGetOrganizationUnitReturns(unitUuid, unit);
            ExpectGetOrganizationUnitReturns(unitUuid, targetUnit);
            ExpectAllowModifyReturns(unit, isUnitValid);
            ExpectAllowModifyReturns(targetUnit, isTargetUnitValid);

            var result = _sut.GetOrganizationRegistrations(orgId,unitId);
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        private void ExpectResolveUuidReturns(int id, Maybe<Guid> result)
        {
            _identityResolverMock.Setup(x => x.ResolveUuid<OrganizationUnit>(id)).Returns(result);
        }

        private void ExpectGetOrganizationUnitReturns(Guid uuid, OrganizationUnit result)
        {
            _organizationServiceMock.Setup(x => x.GetOrganizationUnit(uuid)).Returns(result);
        }

        private void ExpectGetOrganizationUnitReturns(Guid uuid, OperationError result)
        {
            _organizationServiceMock.Setup(x => x.GetOrganizationUnit(uuid)).Returns(result);
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
