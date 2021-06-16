using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries.Interface;
using System.Collections.Generic;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByRightsHolderIdsOrOwnOrganizationIdsTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Id_Matches()
        {
            //Arrange
            var correctId1 = A<int>();
            var correctId2 = A<int>();
            var correctId3 = A<int>();
            var incorrectId = A<int>();

            var matched1 = new ItInterface
            {
                ExhibitedBy = new ItInterfaceExhibit()
                {
                    ItSystem = new ItSystem()
                    {
                        BelongsToId = correctId1
                    }
                }
            };

            var matched2 = new ItInterface
            {
                ExhibitedBy = new ItInterfaceExhibit()
                {
                    ItSystem = new ItSystem()
                    {
                        BelongsToId = correctId2
                    }
                }
            };

            var matched3 = new ItInterface
            {
                OrganizationId = correctId3
            };

            var excludedNoExhibit = new ItInterface { ExhibitedBy = null };

            var excludedWrongUuid = new ItInterface
            {
                ExhibitedBy = new ItInterfaceExhibit()
                {
                    ItSystem = new ItSystem()
                    {
                        BelongsToId = incorrectId
                    }
                }
            };

            var excludedNoRightsHolder = new ItInterface
            {
                ExhibitedBy = new ItInterfaceExhibit()
                {
                    ItSystem = new ItSystem()
                    {
                        BelongsToId = null
                    }
                }
            };

            var input = new[] { excludedWrongUuid, matched1, excludedNoRightsHolder, excludedNoExhibit, matched2, matched3 }.AsQueryable();
            var sut = new QueryByRightsHolderIdsOrOwnOrganizationIds(new List<int>() { correctId1, correctId2 }, new List<int>() { correctId3 });

            //Act
            var result = sut.Apply(input);

            //Assert
            Assert.Equal(3, result.Count());
            var interface1 = result.Where(x => x.ExhibitedBy.ItSystem.BelongsToId == correctId1).First();
            Assert.Same(matched1, interface1);
            var interface2 = result.Where(x => x.ExhibitedBy.ItSystem.BelongsToId == correctId2).First();
            Assert.Same(matched2, interface2);
            var interface3 = result.Where(x => x.OrganizationId == correctId3).First();
            Assert.Same(matched3, interface3);
        }
    }
}
