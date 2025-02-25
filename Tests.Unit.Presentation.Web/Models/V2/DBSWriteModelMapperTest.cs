using Moq;
using Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Presentation.Web.Models.API.V2.Request.System.RightsHolder;
using System.Linq;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class DBSWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly Mock<ICurrentHttpRequest> _currentHttpRequestMock;
        private readonly DBSWriteModelMapper _sut;

        public DBSWriteModelMapperTest()
        {
            _currentHttpRequestMock = new Mock<ICurrentHttpRequest>();
            _currentHttpRequestMock.Setup(x =>
                    x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetAllInputPropertyNames<UpdateDBSPropertiesRequestDTO>());
            _sut = new DBSWriteModelMapper(_currentHttpRequestMock.Object);
        }

        [Fact]
        public void Can_Map_From_PATCH()
        {
            var input = A<UpdateDBSPropertiesRequestDTO>();

            var output = _sut.FromPATCH(input);

            Assert.Equal(input.SystemName, AssertPropertyContainsDataChange(output.SystemName));
            Assert.Equal(input.DataProcessorName, AssertPropertyContainsDataChange(output.DataProcessorName));
        }
    }
}
