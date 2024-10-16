
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
    public class LocalOptionTypeResponseMapperTest: WithAutoFixture
    {
        [Fact]
        public void Can_Map_Regular_Option_Type_DTOs()
        {
            var expected = Enumerable.Range(1, 5)
                .Select(_ => new TestOptionEntity()
                {
                    Uuid = A<Guid>(),
                    Name = A<string>(),
                    Description = A<string>(),
                    IsLocallyAvailable = A<bool>(),
                    IsObligatory = A<bool>()
                }).ToList();
            var sut = new LocalOptionTypeResponseMapper();

            var dtos = sut.ToLocalRegularOptionDTOs<TestReferenceType, TestOptionEntity>(expected);

            expected.ForEach(option =>
            {
                var actual = dtos.First(dto => dto.Uuid == option.Uuid);
                Assert.Equal(option.Name, actual.Name);
                Assert.Equal(option.Description, actual.Description);
                Assert.Equal(option.IsLocallyAvailable, actual.IsActive);
                Assert.Equal(option.IsObligatory, actual.IsObligatory);
            });
        }

        [Fact]
        public void Can_Map_Single_Regular_Option_Type_DTO()
        {
            var expected = new TestOptionEntity()
            {
                Uuid = A<Guid>(),
                Name = A<string>(),
                Description = A<string>(),
                IsLocallyAvailable = A<bool>(),
                IsObligatory = A<bool>()
            };
            var sut = new LocalOptionTypeResponseMapper();

            var actual = sut.ToLocalRegularOptionDTO<TestReferenceType, TestOptionEntity>(expected);

            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.IsLocallyAvailable, actual.IsActive);
            Assert.Equal(expected.IsObligatory, actual.IsObligatory);
        }
    }
}
