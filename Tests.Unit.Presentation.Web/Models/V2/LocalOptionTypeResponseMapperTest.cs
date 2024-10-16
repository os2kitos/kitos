
using System;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2

{
    public class TestOptionEntity : OptionEntity<TestReferenceType> { }
    public class TestReferenceType { }
    public class LocalOptionTypeResponseMapperTest: WithAutoFixture
    {
        private LocalOptionTypeResponseMapper _sut;

        public LocalOptionTypeResponseMapperTest()
        {
            _sut = new LocalOptionTypeResponseMapper();
        }

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

            var dtos = _sut.ToLocalRegularOptionDTOs<TestReferenceType, TestOptionEntity>(expected);

            expected.ForEach(option =>
            {
                var actual = dtos.First(dto => dto.Uuid == option.Uuid);
                AssertRegularOptionTypeDTO(option, actual);
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

            var actual = _sut.ToLocalRegularOptionDTO<TestReferenceType, TestOptionEntity>(expected);

            AssertRegularOptionTypeDTO(expected, actual);
        }

        private void AssertRegularOptionTypeDTO(TestOptionEntity expected, LocalRegularOptionResponseDTO actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.IsLocallyAvailable, actual.IsActive);
            Assert.Equal(expected.IsObligatory, actual.IsObligatory);
        }
    }
}
