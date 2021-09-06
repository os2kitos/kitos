using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries.SystemUsage;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.SystemUsage
{
    public class QueryByRelationToContractTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Id_Match()
        {
            //Arrange
            var correctId = A<Guid>();
            var incorrectId = A<Guid>();
            var matched = CreateWithRelationContractTo(correctId);
            var excluded1 = CreateWithRelationContractTo(incorrectId);
            var excluded2 = CreateWithRelationWithoutContract();

            var input = new[] { matched, excluded1, excluded2 }.AsQueryable();
            var sut = new QueryByRelationToContract(correctId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var entity = Assert.Single(result);
            Assert.Same(matched, entity);
        }

        private static ItSystemUsage CreateWithRelationWithoutContract()
        {
            return new()
            {
                UsedByRelations = new List<SystemRelation>
                {
                    new(new ItSystemUsage())
                    {
                        ToSystemUsage = new ItSystemUsage()
                    }
                },
                UsageRelations = new List<SystemRelation>
                {
                    new(new ItSystemUsage())
                    {
                        ToSystemUsage = new ItSystemUsage()
                    }
                }
            };
        }

        private static ItSystemUsage CreateWithRelationContractTo(Guid uuid)
        {
            return new()
            {
                UsageRelations = new List<SystemRelation>
                {
                    new (new ItSystemUsage())
                    {
                        ToSystemUsage = new ItSystemUsage(),
                        AssociatedContract = new ItContract{ Uuid = uuid }
                    }
                }
            };
        }
    }
}
