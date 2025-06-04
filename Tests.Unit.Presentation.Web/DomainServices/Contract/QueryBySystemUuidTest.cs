using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries.Contract;
using System;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.Contract
{
    public class QueryBySystemUuidTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Id_Match()
        {
            //Arrange
            var correctUuid = A<Guid>();
            var incorrectUuid = A<Guid>();

            var correctContract1 = CreateContract(CreateItContractItSystemUsage(correctUuid));
            var correctContract2 = CreateContract(CreateItContractItSystemUsage(correctUuid), CreateItContractItSystemUsage(incorrectUuid));

            var incorrectContract1 = CreateContract(CreateItContractItSystemUsage(incorrectUuid));
            var incorrectContract2 = CreateContract();

            var input = new[] { correctContract1, correctContract2, incorrectContract1, incorrectContract2 }.AsQueryable();
            var sut = new QueryBySystemUuid(correctUuid);

            //Act
            var result = sut.Apply(input);

            //Assert
            Assert.Equal(2, result.Count());
            var itContract1 = result.First(x => x.Id == correctContract1.Id);
            Assert.Same(correctContract1, itContract1);

            var itContract2 = result.First(x => x.Id == correctContract2.Id);
            Assert.Same(correctContract2, itContract2);
        }

        private ItContract CreateContract(params ItContractItSystemUsage[] associatedSystemUsages)
        {
            return new ItContract
            {
                Id = A<int>(),
                AssociatedSystemUsages = associatedSystemUsages.ToList()
            };
        }

        private ItContractItSystemUsage CreateItContractItSystemUsage(Guid systemUsageUuid)
        {
            return new ItContractItSystemUsage
            {
                ItSystemUsage = new ItSystemUsage
                {
                    ItSystem = new ItSystem
                    {
                        Uuid = systemUsageUuid
                    }
                }
            };
        }
    }
}
