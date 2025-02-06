using System.Linq;
using Moq;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Internal.Request;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2.Internal
{
    public class HelpTextWriteModelMapperTest: WriteModelMapperTestBase
    {

        private readonly HelpTextWriteModelMapper _sut;
        private readonly Mock<ICurrentHttpRequest> _request;

        public HelpTextWriteModelMapperTest()
        {
            _request = new Mock<ICurrentHttpRequest>();
            _sut = new HelpTextWriteModelMapper(_request.Object);
        }

        [Fact]
        public void Can_Map_Create_Parameters()
        {
            var dto = A<HelpTextCreateRequestDTO>();

            var result = _sut.ToCreateParameters(dto);

            Assert.Equivalent(dto, result);
        }

        [Fact]
        public void Can_Map_Update_Parameters()
        {
            var dto = A<HelpTextUpdateRequestDTO>();
            _request.Setup(x =>
                    x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetAllInputPropertyNames<HelpTextUpdateRequestDTO>());

            var result = _sut.ToUpdateParameters(dto);

            Assert.Equal(dto.Description, result.Description.NewValue.Value);
            Assert.Equal(dto.Title, result.Title.NewValue.Value);
        }
    }
}
