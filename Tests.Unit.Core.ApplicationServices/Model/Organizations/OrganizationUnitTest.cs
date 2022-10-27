using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Tests.Toolkit.Patterns;
using Xunit;
using Core.DomainModel.Organization;

namespace Tests.Unit.Core.Model.Organizations
{
    public class OrganizationUnitTest : WithAutoFixture
    {
        [Fact]
        public void RemoveOrganizationUnit_Returns_NotFound()
        {
            var org = new Organization();
            var unitId = A<int>();
            const string expectedErrorMessage = "Unit doesn't exist in the organization";

            var result = org.RemoveOrganizationUnit(unitId);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
            Assert.Equal(expectedErrorMessage, result.Error.Message);
        }

        [Fact]
        public void RemoveOrganizationUnit_Returns_BadState_When_Origin_Is_Not_Kitos()
        {
            var unitId = A<int>();
            var origin = OrganizationUnitOrigin.STS_Organisation;
            var unit = SetupOrganizationUnit(unitId, origin);
            var org = new Organization { OrgUnits = new List<OrganizationUnit> { unit } };
            
            const string expectedErrorMessage = "Unit cannot be deleted";

            var result = org.RemoveOrganizationUnit(unitId);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadState, result.Error.FailureType);
            Assert.Equal(expectedErrorMessage, result.Error.Message);
        }

        [Theory]
        [InlineData(false, false, false, false, true)]
        [InlineData(false, false, false, true, false)]
        [InlineData(false, false, true, false, false)]
        [InlineData(false, true, false, false, false)]
        [InlineData(true, false, false, false, false)]
        [InlineData(true, true, true, true, true)]
        public void RemoveOrganizationUnit_Returns_BadState_When_Unit_Is_In_Use(bool isUsing, bool hasRights, bool hasEconomy, bool isResponsible, bool isDelegated)
        {
            var unitId = A<int>();
            var origin = OrganizationUnitOrigin.Kitos;
            var unit = SetupOrganizationUnit(unitId, origin, isUsing, hasRights, hasEconomy, isResponsible, isDelegated);
            var org = new Organization { OrgUnits = new List<OrganizationUnit> { unit } };
            
            const string expectedErrorMessage = "Unit has assigned registrations";

            var result = org.RemoveOrganizationUnit(unitId);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadState, result.Error.FailureType);
            Assert.Equal(expectedErrorMessage, result.Error.Message);
        }

        [Theory]
        [InlineData(OrganizationUnitOrigin.STS_Organisation, null)]
        [InlineData(OrganizationUnitOrigin.Kitos, 1)]
        public void CanBeDeleted_Returns_False(OrganizationUnitOrigin origin, int? parentId)
        {
            var unitId = A<int>();
            var unit = SetupOrganizationUnit(unitId, origin);
            unit.ParentId = parentId;

            var result = unit.CanBeDeleted();

            Assert.False(result);
        }

        [Fact]
        public void CanBeDeleted_Returns_True()
        {
            var unitId = A<int>();
            var origin = OrganizationUnitOrigin.Kitos;
            var unit = SetupOrganizationUnit(unitId, origin);

            var result = unit.CanBeDeleted();

            Assert.True(result);
        }

        [Fact]
        public void Can_RemoveOrganizationUnit()
        {
            var unitId = A<int>();
            var origin = OrganizationUnitOrigin.Kitos;
            var unit = SetupOrganizationUnit(unitId, origin);
            var org = new Organization { OrgUnits = new List<OrganizationUnit> { unit } };

            var result = org.RemoveOrganizationUnit(unitId);

            Assert.True(result.Ok);
            Assert.Equal(unitId, result.Value.Id);
        }

        private OrganizationUnit SetupOrganizationUnit(int id, OrganizationUnitOrigin origin, 
            bool isUsing = false, bool hasRights = false, bool hasEconomy = false, bool isResponsible = false, bool isDelegated = false)
        {
            var unit = new OrganizationUnit
            {
                Id = id, 
                Origin = origin,
                Using = isUsing ? new List<ItSystemUsageOrgUnitUsage> { new() } : new List<ItSystemUsageOrgUnitUsage>(),
                Rights = hasRights ? new List<OrganizationUnitRight> { new() } : new List<OrganizationUnitRight>(),
                EconomyStreams = hasEconomy ? new List<EconomyStream> { new() } : new List<EconomyStream>(),
                ResponsibleForItContracts = isResponsible ? new List<ItContract> { new() } : new List<ItContract>(),
                DelegatedSystemUsages = isDelegated ? new List<ItSystemUsage> { new() } : new List<ItSystemUsage>()
            };
            
            return unit;
        }
    }
}
