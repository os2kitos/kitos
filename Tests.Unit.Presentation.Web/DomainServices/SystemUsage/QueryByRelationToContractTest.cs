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
            var matched1 = CreateWithRelationContractTo(correctId);
            var matched2 = CreateWithRelationContractFrom(correctId);
            var excluded1 = CreateWithRelationContractTo(incorrectId);
            var excluded2 = CreateWithRelationContractFrom(incorrectId);
            var excluded3 = CreateWithRelationWithoutContract();

            var input = new[] { matched1, matched2, excluded1, excluded2, excluded3 }.AsQueryable();
            var sut = new QueryByRelationToContract(correctId);

            //Act
            var result = sut.Apply(input);

            //Assert
            Assert.Equal(2, result.Count());
            var entity1 = Assert.Single(result.Where(x => x.UsageRelations.Count() > 0));
            Assert.Same(matched1, entity1);

            var entity2 = Assert.Single(result.Where(x => x.UsedByRelations.Count() > 0));
            Assert.Same(matched2, entity2);
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

        private static ItSystemUsage CreateWithRelationContractFrom(Guid uuid)
        {
            return new()
            {
                UsedByRelations = new List<SystemRelation>
                {
                    new(new ItSystemUsage())
                    {
                        ToSystemUsage = new ItSystemUsage(),
                        AssociatedContract = new ItContract {Uuid = uuid}
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
