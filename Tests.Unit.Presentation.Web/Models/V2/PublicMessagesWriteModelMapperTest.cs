using Moq;
using Presentation.Web.Infrastructure.Model.Request;
using System.Linq;
using Presentation.Web.Models.API.V2.Internal.Request;
using Tests.Toolkit.Extensions;
using Xunit;
using System.Collections.Generic;
using Presentation.Web.Controllers.API.V2.Internal.Messages.Mapping;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class PublicMessagesWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly Mock<ICurrentHttpRequest> _currentHttpRequestMock;
        private readonly PublicMessagesWriteModelMapper _sut;

        public PublicMessagesWriteModelMapperTest()
        {
            _currentHttpRequestMock = new Mock<ICurrentHttpRequest>();
            _currentHttpRequestMock
                .Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetAllInputPropertyNames<PublicMessageRequestDTO>());
            _sut = new PublicMessagesWriteModelMapper(_currentHttpRequestMock.Object);
        }

        public static IEnumerable<object[]> GetUndefinedGeneralSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(5);
        }

        [Fact]
        public void FromPATCH_Returns_Correct_Write_Params()
        {
            //Arrange
            var dto = A<PublicMessageRequestDTO>();

            //Act
            var mapping = _sut.FromPATCH(dto);

            //Assert
            AssertPropertyContainsDataChange(mapping.LongDescription);
            AssertPropertyContainsDataChange(mapping.Link);
            AssertPropertyContainsDataChange(mapping.ShortDescription);
            AssertPropertyContainsDataChange(mapping.Status);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedGeneralSectionsInput))]
        public void FromPATCH_Ignores_Undefined_Properties(
            bool noTitle,
            bool noLongDescription,
            bool noShortDescription,
            bool noLink,
            bool noStatus)
        {
            //Arrange
            var emptyInput = new PublicMessageRequestDTO();
            ConfigureInput(noTitle,
                noLongDescription, noShortDescription, noLink, noStatus);

            //Act
            var output = _sut.FromPATCH(emptyInput);

            //Assert that all general properties are mapped correctly
            Assert.Equal(noLongDescription, output.LongDescription.IsUnchanged);
            Assert.Equal(noShortDescription, output.ShortDescription.IsUnchanged);
            Assert.Equal(noLink, output.Link.IsUnchanged);
            Assert.Equal(noStatus, output.Status.IsUnchanged);
        }

        private void ConfigureInput(
            bool noTitle,
            bool noLongDescription,
            bool noShortDescription,
            bool noLink,
            bool noStatus)
        {
            var properties = GetAllInputPropertyNames<PublicMessageRequestDTO>();
            if (noTitle) properties.Remove(nameof(PublicMessageRequestDTO.Title));
            if (noLongDescription) properties.Remove(nameof(PublicMessageRequestDTO.LongDescription));
            if (noShortDescription) properties.Remove(nameof(PublicMessageRequestDTO.ShortDescription));
            if (noLink) properties.Remove(nameof(PublicMessageRequestDTO.Link));
            if (noStatus) properties.Remove(nameof(PublicMessageRequestDTO.Status));

            _currentHttpRequestMock
                .Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(properties);
        }
    }
}
