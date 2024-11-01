using Core.DomainModel;
using System;
using System.Linq;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2.Internal
{
    public class TestRegularOptionEntity : OptionEntity<TestReferenceType> { }
    public class TestReferenceType { }
    public class GlobalOptionTypeResponseMapperTest: WithAutoFixture
    {
        [Fact]
        public void Can_Map_Regular_Option_Type_DTOs()
        {
            var sut = new GlobalOptionTypeResponseMapper();
            var expected = Enumerable.Range(1, 5)
                .Select(_ => new TestRegularOptionEntity()
                {
                    Uuid = A<Guid>(),
                    Name = A<string>(),
                    Description = A<string>(),
                    IsEnabled = A<bool>(),
                    IsObligatory = A<bool>()
                }).ToList();

            var dtos = sut.ToGlobalRegularOptionDTOs<TestRegularOptionEntity, TestReferenceType>(expected);

            expected.ForEach(expectedOption =>
            {
                var actual = dtos.First(dto => dto.Uuid == expectedOption.Uuid);
                AssertRegularOptionTypeDTO(expectedOption, actual);
            });
        }

        private void AssertRegularOptionTypeDTO(OptionEntity<TestReferenceType> expected, GlobalRegularOptionResponseDTO actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.IsObligatory, actual.IsObligatory);
            Assert.Equal(expected.IsEnabled, actual.IsEnabled);
            Assert.Equal(expected.Uuid, actual.Uuid);
        }
    }
}
