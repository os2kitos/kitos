using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries.Interface;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.Interface
{
    public class QueryInterfaceByUsedInOrganizationUuidTest: WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Results_Where_Uuid_Is_Found()
        {
            var correctUuid = A<Guid>();
            var include = CreateItInterface(correctUuid);
            var exclude = CreateItInterface(A<Guid>());
            var input = new[] {include, exclude}.AsQueryable();
            var sut = new QueryInterfaceByUsedInOrganizationWithUuid(correctUuid);

            //Act
            var result = sut.Apply(input);

            //Assert
            var interfaceResult = Assert.Single(result);
            Assert.Equal(include.Id, interfaceResult.Id);
        }

        private ItInterface CreateItInterface(Guid uuid)
        {
            return new ItInterface
            {
                Id = A<int>(),
                AssociatedSystemRelations = new List<SystemRelation>
                {
                    new SystemRelation
                    (
                        new ItSystemUsage
                        {
                            Organization = new Organization
                            {
                                Uuid = uuid
                            }
                        }
                    )
                }
            };
        }
    }
}
