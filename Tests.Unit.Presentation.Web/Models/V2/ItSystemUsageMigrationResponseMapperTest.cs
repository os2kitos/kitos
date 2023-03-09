using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Controllers.API.V2.Internal.ItSystemUsages.Mapping;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.SharedProperties;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItSystemUsageMigrationResponseMapperTest : WithAutoFixture
    {
        private readonly ItSystemUsageMigrationResponseMapper _sut;

        public ItSystemUsageMigrationResponseMapperTest()
        {
            _sut = new ItSystemUsageMigrationResponseMapper();
        }

        [Fact]
        public void Can_MapMigration()
        {
            //Arrange
            var fromUsage = CreateUsage();
            var fromSystem = CreateSystem();
            var toSystem = CreateSystem();
            var affectedContract = CreateContract();
            var affectedRelation = CreateSystemRelation();
            var affectedDpr = CreateDpr();

            var entity = new ItSystemUsageMigration
            (
                fromUsage,
                fromSystem,
                toSystem,
                new List<ItContract> {affectedContract},
                new List<SystemRelation> {affectedRelation},
                new List<DataProcessingRegistration> {affectedDpr}
            );

            //Act
            var result = _sut.MapMigration(entity);

            //Assert
            AssertIdentityNamePairWithDeactivatedStatus(entity.SystemUsage, result.TargetUsage);
            AssertIdentityNamePairWithDeactivatedStatus(entity.FromItSystem, result.FromSystem);
            AssertIdentityNamePairWithDeactivatedStatus(entity.ToItSystem, result.ToSystem);

            var contractDto = Assert.Single(result.AffectedContracts);
            AssertIdentityNamePair(affectedContract, contractDto);

            var relationDto = Assert.Single(result.AffectedRelations);
            AssertIdentityNamePairWithDeactivatedStatus(affectedRelation.ToSystemUsage, relationDto.ToSystemUsage);
            AssertIdentityNamePairWithDeactivatedStatus(affectedRelation.FromSystemUsage, relationDto.FromSystemUsage);
            Assert.Equal(affectedRelation.Description, relationDto.Description);
            AssertIdentityNamePair(affectedRelation.RelationInterface, relationDto.Interface);
            AssertIdentityNamePair(affectedRelation.UsageFrequency, relationDto.FrequencyType);
            AssertIdentityNamePair(affectedRelation.AssociatedContract, relationDto.Contract);

            var dprDto = Assert.Single(result.AffectedDataProcessingRegistrations);
            AssertIdentityNamePair(affectedDpr, dprDto);
        }

        [Fact]
        public void Can_MapUnusedSystems()
        {
            //Arrange
            var system1 = CreateSystem();
            var system2 = CreateSystem();

            //Act
            var result = _sut.MapUnusedSystems(new List<ItSystem> {system1, system2});

            //Assert
            var unusedSystems = result.ToList();
            Assert.Equal(2, unusedSystems.Count);
            var systemDto1 = Assert.Single(unusedSystems, x => x.Uuid == system1.Uuid);
            AssertIdentityNamePairWithDeactivatedStatus(system1, systemDto1);
            var systemDto2 = Assert.Single(unusedSystems, x => x.Uuid == system2.Uuid);
            AssertIdentityNamePairWithDeactivatedStatus(system2, systemDto2);
        }

        private static void AssertIdentityNamePair<TEntity>(TEntity entity, IdentityNamePairResponseDTO dto) where TEntity : class, IHasUuid, IHasName
        {
            Assert.Equal(entity.Uuid, dto.Uuid);
            Assert.Equal(entity.Name, dto.Name);
        }

        private static void AssertIdentityNamePairWithDeactivatedStatus<TEntity>(TEntity entity, IdentityNamePairWithDeactivatedStatusDTO dto) where TEntity : class, IHasUuid, IHasName, IEntityWithEnabledStatus
        {
            AssertIdentityNamePair(entity, dto);
            Assert.Equal(entity.Disabled, dto.Deactivated);
        }

        private static void AssertIdentityNamePairWithDeactivatedStatus(ItSystemUsage entity, IdentityNamePairWithDeactivatedStatusDTO dto)
        {
            Assert.Equal(entity.Uuid, dto.Uuid);
            Assert.Equal(entity.ItSystem.Name, dto.Name);
            Assert.Equal(entity.ItSystem.Disabled, dto.Deactivated);
        }

        private ItSystemUsage CreateUsage()
        {
            return new ItSystemUsage
            {
                Uuid = A<Guid>(),
                ItSystem = CreateSystem()
            };
        }

        private ItSystem CreateSystem()
        {
            return new ItSystem
            {
                Uuid = A<Guid>(),
                Name = A<string>(),
                Disabled = A<bool>()
            };
        }

        private SystemRelation CreateSystemRelation()
        {
            return new SystemRelation(CreateUsage())
            {
                ToSystemUsage = CreateUsage(),
                Description = A<string>(),
                RelationInterface = CreateInterface(),
                UsageFrequency = new RelationFrequencyType{Uuid = A<Guid>(), Name = A<string>()},
                AssociatedContract = CreateContract()
            };
        }

        private ItContract CreateContract()
        {
            return new ItContract
            {
                Uuid = A<Guid>(),
                Name = A<string>(),
            };
        }

        private DataProcessingRegistration CreateDpr()
        {
            return new DataProcessingRegistration
            {
                Uuid = A<Guid>(),
                Name = A<string>(),
            };
        }

        private ItInterface CreateInterface()
        {
            return new ItInterface
            {
                Uuid = A<Guid>(),
                Name = A<string>(),
            };
        }
    }
}
