using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
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
            ExpectGetExternalEconomyStreamsReturns(contract, unit.Id, contract.ExternEconomyStreams);
            ExpectGetInternalEconomyStreamsReturns(contract, unit.Id, contract.InternEconomyStreams);
            
            ExpectGetSystemsByResponsibleUnitIdReturns(unit.Id, systemList);
            ExpectGetSystemsByRelevantUnitIdReturns(unit.Id, systemList);

            var result = _sut.GetOrganizationRegistrations(unit.Id);

            Assert.False(result.Failed);
            var registrations = result.Value;

            Assert.Equal(unit.Rights.Count, registrations.Roles.Count());
            Assert.Contains(right.Id, registrations.Roles.Select(x => x.Id));
            Assert.Contains(right.Role.Name, registrations.Roles.Select(x => x.Text));

            Assert.Equal(contract.ExternEconomyStreams.Count, registrations.ExternalPayments.Count());
            Assert.Contains(externalEconomyStream.Id, registrations.ExternalPayments.Select(x => x.Id));

            Assert.Equal(contract.InternEconomyStreams.Count, registrations.InternalPayments.Count());
            Assert.Contains(internalEconomyStream.Id, registrations.InternalPayments.Select(x => x.Id));

            Assert.Contains(contract.Id, registrations.ContractRegistrations.Select(x => x.Id));
            Assert.Contains(contract.Name, registrations.ContractRegistrations.Select(x => x.Text));

            Assert.Contains(system.Id, registrations.ResponsibleSystemRegistrations.Select(x => x.Id));
            Assert.Contains(system.LocalCallName, registrations.ResponsibleSystemRegistrations.Select(x => x.Text));

            Assert.Contains(system.Id, registrations.RelevantSystemRegistrations.Select(x => x.Id));
            Assert.Contains(system.LocalCallName, registrations.RelevantSystemRegistrations.Select(x => x.Text));
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

        private void ExpectResolveUuidReturns(int id, Guid result)
        {
            _identityResolverMock.Setup(x => x.ResolveUuid<OrganizationUnit>(id)).Returns(result);
        }

        private void ExpectGetOrganizationUnitReturns(Guid uuid, OrganizationUnit result)
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

        private void ExpectGetExternalEconomyStreamsReturns(ItContract contract, int unitId, IEnumerable<EconomyStream> result)
        {
            _economyStreamServiceMock.Setup(x => x.GetExternalEconomyStreamsByUnitId(contract, unitId)).Returns(result);
        }

        private void ExpectGetInternalEconomyStreamsReturns(ItContract contract, int unitId, IEnumerable<EconomyStream> result)
        {
            _economyStreamServiceMock.Setup(x => x.GetInternalEconomyStreamsByUnitId(contract, unitId)).Returns(result);
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
