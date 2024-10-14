
using System;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2

{

    public class TestOptionEntity : OptionEntity<TestReferenceType> { }
    public class TestReferenceType { }
    public class OptionTypeResponseMapperTest: WithAutoFixture
    {
        [Fact]
        public void Can_Map_Regular_Option_Type_DTOs()
        {
            var expected = Enumerable.Range(1, 5)
                .Select(_ => new TestOptionEntity()
                {
                    Uuid = A<Guid>(),
                    Name = A<string>(),
                    Description = A<string>()
                }).ToList();
            var sut = new OptionTypeResponseMapper();

            var dtos = sut.ToRegularOptionDTOs<TestReferenceType, TestOptionEntity>(expected);

            expected.ForEach(option =>
            {
                var actual = dtos.First(dto => dto.Uuid == option.Uuid);
                Assert.Equal(option.Name, actual.Name);
                Assert.Equal(option.Description, actual.Description);
            });
        }
    }
}
