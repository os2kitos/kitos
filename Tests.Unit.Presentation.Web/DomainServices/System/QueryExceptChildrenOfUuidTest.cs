using Core.DomainModel.ItSystem;
using System.Linq;
using System;
using System.Collections.Generic;
using Core.DomainServices.Queries;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.System
{
    public class QueryExceptChildrenOfUuidTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_Without_Excluded_Uuid()
        {
            //Arrange
            var includedUuid = A<Guid>();
            var excludedUuid = A<Guid>();
            var includedSystem = new ItSystem { Uuid = includedUuid};
            var excludedSystem = new ItSystem { Uuid = excludedUuid};

            var input = new[] { includedSystem, excludedSystem }.AsQueryable();
            var sut = new QueryExceptEntitiesWithUuids<ItSystem>(new List<Guid>{ excludedUuid });

            //Act
            var result = sut.Apply(input);

            //Assert
            var itSystem = Assert.Single(result);
            Assert.Equal(includedUuid, itSystem.Uuid);
        }
    }
}
