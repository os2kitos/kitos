using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var correctContract1 = new ItContract
            {
                Id = A<int>(),
                AssociatedSystemUsages = new List<ItContractItSystemUsage>
                {
                    new ItContractItSystemUsage
                    {
                        ItSystemUsage = new ItSystemUsage
                        {
                            ItSystem = new ItSystem
                            {
                                Uuid = correctUuid
                            }
                        }
                    }
                }
            };

            var correctContract2 = new ItContract
            {
                Id = A<int>(),
                AssociatedSystemUsages = new List<ItContractItSystemUsage>
                {
                    new ItContractItSystemUsage
                    {
                        ItSystemUsage = new ItSystemUsage
                        {
                            ItSystem = new ItSystem
                            {
                                Uuid = correctUuid
                            }
                        }
                    },
                    new ItContractItSystemUsage
                    {
                        ItSystemUsage = new ItSystemUsage
                        {
                            ItSystem = new ItSystem
                            {
                                Uuid = incorrectUuid
                            }
                        }
                    }
                }
            };

            var incorrectContract1 = new ItContract
            {
                AssociatedSystemUsages = new List<ItContractItSystemUsage>
                {
                    new ItContractItSystemUsage
                    {
                        ItSystemUsage = new ItSystemUsage
                        {
                            ItSystem = new ItSystem
                            {
                                Uuid = incorrectUuid
                            }
                        }
                    }
                }
            };
            var incorrectContract2 = new ItContract
            {
                AssociatedSystemUsages = new List<ItContractItSystemUsage>
                {
                    new ItContractItSystemUsage
                    {
                        ItSystemUsage = new ItSystemUsage
                        {
                        }
                    }
                }
            };
            var incorrectContract3 = new ItContract
            {
                AssociatedSystemUsages = new List<ItContractItSystemUsage>
                {
                    new ItContractItSystemUsage
                    {
                    }
                }
            };

            var incorrectContract4 = new ItContract
            {
                AssociatedSystemUsages = new List<ItContractItSystemUsage>
                {
                }
            };

            var input = new[] { correctContract1, correctContract2, incorrectContract1, incorrectContract2, incorrectContract3, incorrectContract4 }.AsQueryable();
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
    }
}
