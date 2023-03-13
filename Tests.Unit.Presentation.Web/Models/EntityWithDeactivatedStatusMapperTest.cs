using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models
{
    public class EntityWithDeactivatedStatusMapperTest: WithAutoFixture
    {
        private readonly EntityWithDeactivatedStatusMapper _sut = new ();

        [Fact]
        public void Can_MapUnusedSystems()
        {
            //Arrange
            var system1 = CreateSystem();
            var system2 = CreateSystem();

            //Act
            var result = _sut.Map(new List<ItSystem> { system1, system2 });

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

        private ItSystem CreateSystem()
        {
            return new ItSystem
            {
                Uuid = A<Guid>(),
                Name = A<string>(),
                Disabled = A<bool>()
            };
        }
    }
}
