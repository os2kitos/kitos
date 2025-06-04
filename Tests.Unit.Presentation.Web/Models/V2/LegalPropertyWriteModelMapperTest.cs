using Moq;
using Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using System.Linq;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class LegalPropertyWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly Mock<ICurrentHttpRequest> _currentHttpRequestMock;
        private readonly LegalPropertyWriteModelMapper _sut;

        public LegalPropertyWriteModelMapperTest()
        {
            _currentHttpRequestMock = new Mock<ICurrentHttpRequest>();
            _currentHttpRequestMock.Setup(x =>
                    x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetAllInputPropertyNames<LegalPropertiesUpdateRequestDTO>());
            _sut = new LegalPropertyWriteModelMapper(_currentHttpRequestMock.Object);
        }

        [Fact]
        public void Can_Map_From_PATCH()
        {
            var input = A<LegalPropertiesUpdateRequestDTO>();

            var output = _sut.FromPATCH(input);

            Assert.Equal(input.SystemName, AssertPropertyContainsDataChange(output.SystemName));
            Assert.Equal(input.DataProcessorName, AssertPropertyContainsDataChange(output.DataProcessorName));
        }
    }
}
