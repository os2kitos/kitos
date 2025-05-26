using Core.DomainModel.ItContract;
using Core.DomainServices.Queries.Contract;
using System;
using System.Linq;
using Core.DomainModel.Organization;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.Contract
{
    public class QueryByResponsibleOrganizationUnitUuidTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Id_Match()
        {
            //Arrange
            var correctUuid = A<Guid>();
            var incorrectUuid = A<Guid>();

            var correctContract = CreateContractWithResponsibleOrgUni(correctUuid);

            var incorrectContract1 = CreateContractWithResponsibleOrgUni(incorrectUuid);
            var incorrectContract2 = new ItContract();

            var input = new[] { correctContract, incorrectContract1, incorrectContract2 }.AsQueryable();
            var sut = new QueryByResponsibleOrganizationUnitUuid(correctUuid);

            //Act
            var result = sut.Apply(input);

            //Assert
            var matched = Assert.Single(result);
            Assert.Same(correctContract, matched);
        }

        private ItContract CreateContractWithResponsibleOrgUni(Guid uuid)
        {
            return new ItContract
            {
                Id = A<int>(),
                ResponsibleOrganizationUnit = new OrganizationUnit { Uuid = uuid }
            };
        }
    }
}
