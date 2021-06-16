using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries.Interface;
using System;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.Interface
{
    public class QueryByExposingSystemTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_Exposing_System_Uuid_Match()
        {
            //Arrange
            var correctId = A<Guid>();
            var incorrectId = A<Guid>();
            var matched = new ItInterface
            {
                ExhibitedBy = new ItInterfaceExhibit()
                {
                    ItSystem = new ItSystem()
                    {
                        Uuid = correctId
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
                        Uuid = incorrectId
                    }
                }
            };

            var input = new[] { excludedWrongUuid, matched, excludedNoExhibit }.AsQueryable();
            var sut = new QueryByExposingSystem(correctId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var itInterface = Assert.Single(result);
            Assert.Same(matched, itInterface);
        }
    }
}
