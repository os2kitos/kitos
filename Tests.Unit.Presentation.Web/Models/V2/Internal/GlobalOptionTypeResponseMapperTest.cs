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

    public class TestRoleOptionEntity: OptionEntity<TestReferenceType>, IRoleEntity
    {
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
    }

    public class GlobalOptionTypeResponseMapperTest: WithAutoFixture
    {
        private readonly GlobalOptionTypeResponseMapper _sut;

        public GlobalOptionTypeResponseMapperTest()
        {
            _sut = new GlobalOptionTypeResponseMapper();
        }

        [Fact]
        public void Can_Map_Single_Role_Option_DTO()
        {
            var expected = new TestRoleOptionEntity()
            {
                Uuid = A<Guid>(),
                Name = A<string>(),
                Description = A<string>(),
                IsEnabled = A<bool>(),
                IsObligatory = A<bool>(),
                HasWriteAccess = A<bool>()
            };

            var dto = _sut.ToGlobalRoleOptionDTO<TestRoleOptionEntity, TestReferenceType>(expected);

            AssertRoleOptionTypeDTO(expected, dto);
        }

        [Fact]
        public void Can_Map_Role_Option_Type_DTOs()
        {
            var expected = Enumerable.Range(1, 5)
                .Select(_ => new TestRoleOptionEntity()
                {
                    Uuid = A<Guid>(),
                    Name = A<string>(),
                    Description = A<string>(),
                    IsEnabled = A<bool>(),
                    IsObligatory = A<bool>(),
                    HasWriteAccess = A<bool>()
                }).ToList();

            var dtos = _sut.ToGlobalRoleOptionDTOs<TestRoleOptionEntity, TestReferenceType>(expected);

            expected.ForEach(expectedOption =>
            {
                var actual = dtos.First(dto => dto.Uuid == expectedOption.Uuid);
                AssertRoleOptionTypeDTO(expectedOption, actual);
            });
        }

        [Fact]
        public void Can_Map_Single_Regular_Option_DTO()
        {
            var expected = new TestRegularOptionEntity()
            {
                Uuid = A<Guid>(),
                Name = A<string>(),
                Description = A<string>(),
                IsEnabled = A<bool>(),
                IsObligatory = A<bool>()
            };

            var dto = _sut.ToGlobalRegularOptionDTO<TestRegularOptionEntity, TestReferenceType>(expected);

            AssertRegularOptionTypeDTO(expected, dto);
        }

        [Fact]
        public void Can_Map_Regular_Option_Type_DTOs()
        {
            var expected = Enumerable.Range(1, 5)
                .Select(_ => new TestRegularOptionEntity()
                {
                    Uuid = A<Guid>(),
                    Name = A<string>(),
                    Description = A<string>(),
                    IsEnabled = A<bool>(),
                    IsObligatory = A<bool>()
                }).ToList();

            var dtos = _sut.ToGlobalRegularOptionDTOs<TestRegularOptionEntity, TestReferenceType>(expected);

            expected.ForEach(expectedOption =>
            {
                var actual = dtos.First(dto => dto.Uuid == expectedOption.Uuid);
                AssertRegularOptionTypeDTO(expectedOption, actual);
            });
        }

        private void AssertRoleOptionTypeDTO(TestRoleOptionEntity expected, GlobalRoleOptionResponseDTO actual)
        {
            AssertRegularOptionTypeDTO(expected, actual);
            Assert.Equal(expected.HasWriteAccess, actual.WriteAccess);
        }
        private void AssertRegularOptionTypeDTO(OptionEntity<TestReferenceType> expected, GlobalRegularOptionResponseDTO actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.IsObligatory, actual.IsObligatory);
            Assert.Equal(expected.IsEnabled, actual.IsEnabled);
            Assert.Equal(expected.Uuid, actual.Uuid);
            Assert.Equal(expected.Priority, actual.Priority);
        }
    }
}
