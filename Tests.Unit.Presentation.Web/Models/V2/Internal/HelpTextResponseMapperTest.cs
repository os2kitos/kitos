using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2.Internal
{
    public class HelpTextResponseMapperTest: WithAutoFixture
    {

        [Fact]
        public void Can_Map_Help_Texts()
        {
            var sut = new HelpTextResponseMapper();
            var expected = new HelpText()
            {
                Description = A<string>(),
                Title = A<string>(),
                Key = A<string>()
            };
            var expectedList = new List<HelpText>()
            {
                expected
            };

            var dtos = sut.ToResponseDTOs(expectedList);

            Assert.Single(dtos);
            var actual = dtos.FirstOrDefault();
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.Key, actual.Key);
        }
    }
}
