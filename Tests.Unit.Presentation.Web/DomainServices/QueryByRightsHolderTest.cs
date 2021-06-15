using System;
using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByRightsHolderTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Returns_Items_With_RightsHolder_Uuid_Match()
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
                        BelongsTo = new Organization()
                        {
                            Uuid = correctId
                        }
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
                        BelongsTo = new Organization()
                        {
                            Uuid = incorrectId
                        }
                    }
                }
            };

            var excludedNoRightsHolder = new ItInterface
            {
                ExhibitedBy = new ItInterfaceExhibit()
                {
                    ItSystem = new ItSystem()
                    {
                        BelongsTo = null
                    }
                }
            };

            var input = new[] { excludedWrongUuid, matched, excludedNoExhibit, excludedNoRightsHolder }.AsQueryable();
            var sut = new Core.DomainServices.Queries.Interface.QueryByRightsHolder(correctId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var itInterface = Assert.Single(result);
            Assert.Same(matched, itInterface);
        }


        [Fact]
        public void Apply_Returns_Items_With_Uuid_Match()
        {
            //Arrange
            var correctId = A<Guid>();
            var incorrectId = A<Guid>();
            var matched = new ItSystem { BelongsTo = new Organization() { Uuid = correctId } };
            var excludedNoRightsHolder = new ItSystem { BelongsTo = null };
            var excludedWrongUuid = new ItSystem { BelongsTo = new Organization { Uuid = incorrectId } };

            var input = new[] { excludedWrongUuid, matched, excludedNoRightsHolder }.AsQueryable();
            var sut = new Core.DomainServices.Queries.ItSystem.QueryByRightsHolder(correctId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var itSystem = Assert.Single(result);
            Assert.Same(matched, itSystem);
        }
    }
}

