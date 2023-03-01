using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries.ItSystem;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.System
{
    public class QuerySystemByUsedInOrganizationUuidTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Results_Where_Uuid_Is_Found()
        {
            //Arrange
            var correctUuid = A<Guid>();
            var include = CreateItSystem(correctUuid);
            var exclude = CreateItSystem(A<Guid>());
            var input = new[] {include, exclude}.AsQueryable();
            var sut = new QuerySystemByUsedInOrganizationUuid(correctUuid);

            //Act
            var result = sut.Apply(input).ToList();

            //Assert
            var systemResult = Assert.Single(result);
            Assert.Equal(include.Id, systemResult.Id);
        }

        private ItSystem CreateItSystem(Guid uuid)
        {
            return new ItSystem
            {
                Id = A<int>(),
                Usages = new List<ItSystemUsage>
                {
                    new(){Organization = new Organization {Uuid = uuid}}
                }
            };
        }
    }
}
