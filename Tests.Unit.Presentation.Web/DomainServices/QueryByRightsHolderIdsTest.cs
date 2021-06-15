using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries.Interface;
using System.Collections.Generic;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByRightsHolderIdsTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Id_Matches()
        {
            //Arrange
            var correctId1 = A<int>();
            var correctId2 = A<int>();
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

            var input = new[] { excludedWrongUuid, matched1, excludedNoRightsHolder, excludedNoExhibit, matched2 }.AsQueryable();
            var sut = new QueryByRightsHolderIds(new List<int>() { correctId1, correctId2 });

            //Act
            var result = sut.Apply(input);

            //Assert
            Assert.Equal(2, result.Count());
            var interface1 = result.Where(x => x.ExhibitedBy.ItSystem.BelongsToId == correctId1).First();
            Assert.Same(matched1, interface1);
            var interface2 = result.Where(x => x.ExhibitedBy.ItSystem.BelongsToId == correctId2).First();
            Assert.Same(matched2, interface2);
            
        }
    }
}
