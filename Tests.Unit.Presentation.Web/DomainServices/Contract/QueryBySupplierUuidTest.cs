using Core.DomainModel.ItContract;
using Core.DomainServices.Queries.Contract;
using System;
using System.Linq;
using Core.DomainModel.Organization;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.Contract
{
    public class QueryBySupplierUuidTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Id_Match()
        {
            //Arrange
            var correctUuid = A<Guid>();
            var incorrectUuid = A<Guid>();

            var correctContract = CreateContractWithSupplier(correctUuid);

            var incorrectContract1 = CreateContractWithSupplier(incorrectUuid);
            var incorrectContract2 = new ItContract();

            var input = new[] { correctContract, incorrectContract1, incorrectContract2 }.AsQueryable();
            var sut = new QueryBySupplierUuid(correctUuid);

            //Act
            var result = sut.Apply(input);

            //Assert
            var matched = Assert.Single(result);
            Assert.Same(correctContract, matched);
        }

        private ItContract CreateContractWithSupplier(Guid uuid)
        {
            return new ItContract
            {
                Id = A<int>(),
                Supplier = new Organization { Uuid = uuid }
            };
        }
    }
}
