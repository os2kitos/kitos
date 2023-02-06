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
                .Returns(GetAllInputPropertyNames<PublicMessagesRequestDTO>());
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
            var dto = A<PublicMessagesRequestDTO>();

            //Act
            var mapping = _sut.FromPATCH(dto);

            //Assert
            Assert.Equal(dto.About, AssertPropertyContainsDataChange(mapping.About));
            Assert.Equal(dto.ContactInfo, AssertPropertyContainsDataChange(mapping.ContactInfo));
            Assert.Equal(dto.Misc, AssertPropertyContainsDataChange(mapping.Misc));
            Assert.Equal(dto.Guides, AssertPropertyContainsDataChange(mapping.Guides));
            Assert.Equal(dto.StatusMessages, AssertPropertyContainsDataChange(mapping.StatusMessages));
        }

        [Theory]
        [MemberData(nameof(GetUndefinedGeneralSectionsInput))]
        public void FromPATCH_Ignores_Undefined_Properties(
            bool noAbout,
            bool noGuide,
            bool noContactInfo,
            bool noMisc,
            bool noStatusMessages)
        {
            //Arrange
            var emptyInput = new PublicMessagesRequestDTO();
            ConfigureInput(
                noAbout,
                noGuide,
                noContactInfo,
                noMisc,
                noStatusMessages);

            //Act
            var output = _sut.FromPATCH(emptyInput);

            //Assert that all general properties are mapped correctly
            Assert.Equal(noAbout, output.About.IsUnchanged);
            Assert.Equal(noGuide, output.Guides.IsUnchanged);
            Assert.Equal(noStatusMessages, output.StatusMessages.IsUnchanged);
            Assert.Equal(noContactInfo, output.ContactInfo.IsUnchanged);
            Assert.Equal(noMisc, output.Misc.IsUnchanged);
        }

        private void ConfigureInput(
            bool noAbout,
            bool noGuide,
            bool noContactInfo,
            bool noMisc,
            bool noStatusMessages)
        {
            var properties = GetAllInputPropertyNames<PublicMessagesRequestDTO>();
            if (noAbout) properties.Remove(nameof(PublicMessagesRequestDTO.About));
            if (noMisc) properties.Remove(nameof(PublicMessagesRequestDTO.Misc));
            if (noContactInfo) properties.Remove(nameof(PublicMessagesRequestDTO.ContactInfo));
            if (noStatusMessages) properties.Remove(nameof(PublicMessagesRequestDTO.StatusMessages));
            if (noGuide) properties.Remove(nameof(PublicMessagesRequestDTO.Guides));

            _currentHttpRequestMock
                .Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(properties);
        }
    }
}
