using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Core.DomainServices;
using Tests.Toolkit.Patterns;
using Moq;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Organizations
{
    public class OrganizationRegistrationServiceTest : WithAutoFixture
    {
        private readonly OrganizationRegistrationService _sut;

        private readonly Mock<IEntityIdentityResolver> _identityResolverMock;
        private readonly Mock<IOrganizationService> _organizationServiceMock;
        private readonly Mock<IOrganizationRightsService> _organizationRightsServiceMock;
        private readonly Mock<IEconomyStreamService> _economyStreamServiceMock;
        private readonly Mock<IItContractService> _contractServiceMock;
        private readonly Mock<IItSystemUsageService> _usageServiceMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IOrgUnitService> _orgUnitServiceMock;

        public OrganizationRegistrationServiceTest()
        {
            _identityResolverMock = new Mock<IEntityIdentityResolver>();
            _organizationServiceMock = new Mock<IOrganizationService>();
            _organizationRightsServiceMock = new Mock<IOrganizationRightsService>();
            _economyStreamServiceMock = new Mock<IEconomyStreamService>();
            _contractServiceMock = new Mock<IItContractService>();
            _usageServiceMock = new Mock<IItSystemUsageService>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _orgUnitServiceMock = new Mock<IOrgUnitService>();

            _sut = new OrganizationRegistrationService(
                _identityResolverMock.Object,
                _organizationServiceMock.Object,
                _organizationRightsServiceMock.Object,
                _economyStreamServiceMock.Object,
                _contractServiceMock.Object,
                _usageServiceMock.Object,
                _authorizationContextMock.Object,
                _orgUnitServiceMock.Object);
        }

        [Fact]
        public void GetOrganizationRegistrations_Returns_Registrations()
        {
            var unit = CreateOrganizationUnit();
            var unitUuid = A<Guid>();
            var right = CreateOrganizationUnitRight(unit.Id);
            right.Role = CreateOrganizationUnitRole();

            var rights = new List<OrganizationUnitRight> { right };
            unit.Rights = rights;
            
            var contract = CreateContract(unit.Id);
            var contractList = new List<ItContract>() { contract };
            var internalEconomyStream = new EconomyStream { Id = A<int>(), OrganizationUnitId = unit.Id };
            var externalEconomyStream = new EconomyStream { Id = A<int>(), OrganizationUnitId = unit.Id };
            contract.ExternEconomyStreams = new List<EconomyStream> { externalEconomyStream };
            contract.InternEconomyStreams = new List<EconomyStream> { internalEconomyStream };
            contract.ResponsibleOrganizationUnit = unit;

            var system = CreateSystem(unit.Id);
            var systemList = new List<ItSystemUsage> { system };
            var relevantSystem = CreateItSystemUsageOrgUnitUsage(unit, system);
            system.UsedBy = new List<ItSystemUsageOrgUnitUsage> { relevantSystem };
            system.ResponsibleUsage = CreateItSystemUsageOrgUnitUsage(unit, system);

            ExpectResolveUuidReturns(unit.Id, unitUuid);
            ExpectGetOrganizationUnitReturns(unitUuid, unit);
            ExpectAllowReadsReturns(unit, true);

            ExpectGetContractsByResponsibleUnitIdReturns(unit.Id, contractList.AsQueryable());
            
            ExpectGetSystemsByResponsibleUnitIdReturns(unit.Id, systemList);
            ExpectGetSystemsByRelevantUnitIdReturns(unit.Id, systemList);

            var result = _sut.GetOrganizationRegistrations(unit.Id);

            Assert.False(result.Failed);
            var registrations = result.Value;
            /*var roles = registrations.Where(x => x.Type == OrganizationRegistrationType.Roles).ToList();
            var externalPayments = registrations.Where(x => x.Type == OrganizationRegistrationType.ExternalPayments).ToList();
            var internalPayments = registrations.Where(x => x.Type == OrganizationRegistrationType.InternalPayments).ToList();
            var contractRegistrations = registrations.Where(x => x.Type == OrganizationRegistrationType.ContractRegistrations).ToList();
            var responsibleSystems = registrations.Where(x => x.Type == OrganizationRegistrationType.ResponsibleSystems).ToList();
            var relevantSystems = registrations.Where(x => x.Type == OrganizationRegistrationType.RelevantSystems).ToList();

            Assert.Single(roles);
            Assert.Contains(right.Id, roles.Select(x => x.Id));
            Assert.Contains(right.Role.Name, roles.Select(x => x.Text));

            Assert.Single(externalPayments);
            Assert.Contains(externalEconomyStream.Id, externalPayments.Select(x => x.Id));

            Assert.Single(internalPayments);
            Assert.Contains(internalEconomyStream.Id, internalPayments.Select(x => x.Id));

            Assert.Single(contractRegistrations);
            Assert.Contains(contract.Id, contractRegistrations.Select(x => x.Id));
            Assert.Contains(contract.Name, contractRegistrations.Select(x => x.Text));

            Assert.Single(responsibleSystems);
            Assert.Contains(system.Id, responsibleSystems.Select(x => x.Id));
            Assert.Contains(system.LocalCallName, responsibleSystems.Select(x => x.Text));

            Assert.Single(relevantSystems);
            Assert.Contains(system.Id, relevantSystems.Select(x => x.Id));
            Assert.Contains(system.LocalCallName, relevantSystems.Select(x => x.Text));*/
        }

        [Fact]
        public void GetOrganizationRegistrations_Returns_BadInput()
        {
            var unitId = A<int>();

            ExpectResolveUuidReturns(unitId, Maybe<Guid>.None);

            var result = _sut.GetOrganizationRegistrations(unitId);
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            Assert.Equal("Organization id is invalid", result.Error.Message);
        }

        [Fact]
        public void GetOrganizationRegistrations_Returns_NotFound()
        {
            var unitId = A<int>();
            var unitUuid = A<Guid>();
            var operationError = new OperationError("Organization not found", OperationFailure.NotFound);

            ExpectResolveUuidReturns(unitId, unitUuid);
            ExpectGetOrganizationUnitReturns(unitUuid, operationError);

            var result = _sut.GetOrganizationRegistrations(unitId);
            Assert.True(result.Failed);
            Assert.Equal(operationError.FailureType, result.Error.FailureType);
            Assert.Equal(operationError.Message, result.Error.Message);
        }

        [Fact]
        public void GetOrganizationRegistrations_Returns_Forbidden()
        {
            var unitId = A<int>();
            var unitUuid = A<Guid>();
            var unit = new OrganizationUnit() {Id = unitId, Uuid = unitUuid};

            ExpectResolveUuidReturns(unitId, unitUuid);
            ExpectGetOrganizationUnitReturns(unitUuid, unit);
            ExpectAllowReadsReturns(unit, false);

            var result = _sut.GetOrganizationRegistrations(unitId);
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void DeleteSelectedOrganizationRegistrations_Returns_BadInput()
        {
            var unitId = A<int>();

            ExpectResolveUuidReturns(unitId, Maybe<Guid>.None);

            var result = _sut.DeleteSelectedOrganizationRegistrations(unitId, new OrganizationRegistrationChangeParameters());
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
            Assert.Equal("Organization id is invalid", result.Value.Message);
        }

        [Fact]
        public void DeleteSelectedOrganizationRegistrations_Returns_NotFound()
        {
            var unitId = A<int>();
            var unitUuid = A<Guid>();
            var operationError = new OperationError("Organization not found", OperationFailure.NotFound);

            ExpectResolveUuidReturns(unitId, unitUuid);
            ExpectGetOrganizationUnitReturns(unitUuid, operationError);

            var result = _sut.DeleteSelectedOrganizationRegistrations(unitId, new OrganizationRegistrationChangeParameters());
            Assert.True(result.HasValue);
            Assert.Equal(operationError.FailureType, result.Value.FailureType);
            Assert.Equal(operationError.Message, result.Value.Message);
        }

        [Fact]
        public void DeleteSelectedOrganizationRegistrations_Returns_Forbidden()
        {
            var unitId = A<int>();
            var unitUuid = A<Guid>();
            var unit = new OrganizationUnit() {Id = unitId, Uuid = unitUuid};

            ExpectResolveUuidReturns(unitId, unitUuid);
            ExpectGetOrganizationUnitReturns(unitUuid, unit);
            ExpectAllowDeleteReturns(unit, false);

            var result = _sut.DeleteSelectedOrganizationRegistrations(unitId, new OrganizationRegistrationChangeParameters());
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Forbidden, result.Value.FailureType);
        }

        [Theory]
        [InlineData(false, true, "Organization id is invalid")]
        [InlineData(true, false, "Target organization id is invalid")]
        public void TransferSelectedOrganizationRegistrations_Returns_BadInput(bool isUnitValid, bool isTargetUnitValid, string expectedMessage)
        {
            var unitId = A<int>();
            var targetUnitId = A<int>();

            ExpectResolveUuidReturns(unitId, isUnitValid ? A<Guid>() :Maybe<Guid>.None);
            ExpectResolveUuidReturns(targetUnitId, isTargetUnitValid ? A<Guid>() : Maybe<Guid>.None);

            var result = _sut.TransferSelectedOrganizationRegistrations(unitId, targetUnitId, new OrganizationRegistrationChangeParameters());
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
            Assert.Equal(expectedMessage, result.Value.Message);
        }

        [Theory]
        [InlineData(false, true, "Organization not found")]
        [InlineData(true, false, "Target organization not found")]
        public void TransferSelectedOrganizationRegistrations_Returns_NotFound(bool isUnitValid, bool isTargetUnitValid, string expectedMessage)
        {
            var unitId = A<int>();
            var targetUnitId = A<int>();
            var unitUuid = A<Guid>();
            var targetUnitIdUuid = A<Guid>();
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
            
            var result = _sut.TransferSelectedOrganizationRegistrations(unitId, targetUnitId, new OrganizationRegistrationChangeParameters());
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

            var result = _sut.GetOrganizationRegistrations(unitId);
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        private OrganizationUnit CreateOrganizationUnit()
        {
            return new OrganizationUnit
            {
                Id = A<int>(),
                Name = A<string>()
            };
        }

        private OrganizationUnitRight CreateOrganizationUnitRight(int unitId)
        {
            return new OrganizationUnitRight
            {
                Id = A<int>(),
                ObjectId = unitId,
                Role = new OrganizationUnitRole
                {
                    Name = A<string>()
                }
            };
        }

        private OrganizationUnitRole CreateOrganizationUnitRole()
        {
            return new OrganizationUnitRole
            {
                Name = A<string>()
            };
        }

        private ItContract CreateContract(int unitId)
        {
            return new ItContract
            {
                Id = A<int>(),
                ResponsibleOrganizationUnitId = unitId,
                Name = A<string>()
            };
        }

        private ItSystemUsage CreateSystem(int unitId)
        {
            return new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = unitId,
                LocalCallName = A<string>()
            };
        }

        private ItSystemUsageOrgUnitUsage CreateItSystemUsageOrgUnitUsage(OrganizationUnit unit, ItSystemUsage system)
        {
            return new ItSystemUsageOrgUnitUsage
            {
                OrganizationUnit = unit,
                OrganizationUnitId = unit.Id,
                ItSystemUsage = system,
                ItSystemUsageId = system.Id
            };
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

        private void ExpectGetContractsByResponsibleUnitIdReturns(int unitId, IEnumerable<ItContract> result)
        {
            _contractServiceMock.Setup(x => x.GetContractsByResponsibleUnitId(unitId)).Returns(result);
        }

        private void ExpectGetSystemsByRelevantUnitIdReturns(int unitId, IEnumerable<ItSystemUsage> result)
        {
            _usageServiceMock.Setup(x => x.GetSystemsByRelevantUnitId(unitId)).Returns(result);
        }

        private void ExpectGetSystemsByResponsibleUnitIdReturns(int unitId, IEnumerable<ItSystemUsage> result)
        {
            _usageServiceMock.Setup(x => x.GetSystemsByResponsibleUnitId(unitId)).Returns(result);
        }
    }
}
