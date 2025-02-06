
using System;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response;
using Presentation.Web.Models.API.V2.Internal.Response.LocalOptions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2

{
    public class TestRegularOptionEntity : OptionEntity<TestReferenceType> { }
    public class TestReferenceType { }
    public class TestRoleOptionEntity : OptionEntity<TestReferenceType>, IRoleEntity
    {
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
    }

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
                .Select(_ => new TestRegularOptionEntity()
                {
                    Uuid = A<Guid>(),
                    Name = A<string>(),
                    Description = A<string>(),
                    IsLocallyAvailable = A<bool>(),
                    IsObligatory = A<bool>()
                }).ToList();

            var dtos = _sut.ToLocalRegularOptionDTOs<TestReferenceType, TestRegularOptionEntity>(expected);

            expected.ForEach(expectedOption =>
            {
                var actual = dtos.First(dto => dto.Uuid == expectedOption.Uuid);
                AssertRegularOptionTypeDTO(expectedOption, actual);
            });
        }

        [Fact]
        public void Can_Map_Single_Regular_Option_Type_DTO()
        {
            var expected = new TestRegularOptionEntity()
            {
                Uuid = A<Guid>(),
                Name = A<string>(),
                Description = A<string>(),
                IsLocallyAvailable = A<bool>(),
                IsObligatory = A<bool>()
            };

            var actual = _sut.ToLocalRegularOptionDTO<TestReferenceType, TestRegularOptionEntity>(expected);

            AssertRegularOptionTypeDTO(expected, actual);
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
                    IsLocallyAvailable = A<bool>(),
                    IsObligatory = A<bool>(),
                    HasWriteAccess = A<bool>()
                }).ToList();

            var dtos = _sut.ToLocalRoleOptionDTOs<TestReferenceType, TestRoleOptionEntity>(expected);

            expected.ForEach(expectedOption =>
            {
                var actual = dtos.First(dto => dto.Uuid == expectedOption.Uuid);
                AssertRoleOptionTypeDTO(expectedOption, actual);
            });
        }

        [Fact]
        public void Can_Map_Single_Role_Option_Type_DTO()
        {
            var expected = new TestRoleOptionEntity()
            {
                Uuid = A<Guid>(),
                Name = A<string>(),
                Description = A<string>(),
                IsLocallyAvailable = A<bool>(),
                IsObligatory = A<bool>(),
                HasWriteAccess = A<bool>()
            };

            var actual = _sut.ToLocalRoleOptionDTO<TestReferenceType, TestRoleOptionEntity>(expected);

            AssertRoleOptionTypeDTO(expected, actual);
        }

        private void AssertRegularOptionTypeDTO(OptionEntity<TestReferenceType> expected, LocalRegularOptionResponseDTO actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.IsLocallyAvailable, actual.IsLocallyAvailable);
            Assert.Equal(expected.IsObligatory || expected.IsLocallyAvailable, actual.IsActive);
            Assert.Equal(expected.IsObligatory, actual.IsObligatory);
        }

        private void AssertRoleOptionTypeDTO(TestRoleOptionEntity expected, LocalRoleOptionResponseDTO actual)
        {
            AssertRegularOptionTypeDTO(expected, actual);
            Assert.Equal(expected.HasWriteAccess, actual.WriteAccess);
        }
    }
}
