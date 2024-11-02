
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using System.Linq;
using Xunit;
using Moq;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Tests.Toolkit.Extensions;

namespace Tests.Unit.Presentation.Web.Models.V2.Internal
{
    

    public class GlobalOptionTypeWriteModelMapperTest: WriteModelMapperTestBase
    {
            private readonly GlobalOptionTypeWriteModelMapper _sut;
            private readonly Mock<ICurrentHttpRequest> _httpRequest;
        public GlobalOptionTypeWriteModelMapperTest()
        {
            _httpRequest = new Mock<ICurrentHttpRequest>();
            _sut = new GlobalOptionTypeWriteModelMapper(_httpRequest.Object);
        }

        [Fact]
        public void Can_Map_Create_Dto()
        {
            var dto = new GlobalOptionCreateRequestDTO()
            {
                Name = A<string>(),
                Description = A<string>(),
                IsObligatory = A<bool>(),
            };
            _httpRequest.Setup(x =>
                    x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetAllInputPropertyNames<GlobalOptionCreateRequestDTO>());

            var parameters = _sut.ToGlobalOptionCreateParameters(dto);

            Assert.Equal(dto.Name, parameters.Name);
            Assert.Equal(dto.Description, parameters.Description);
            Assert.Equal(dto.IsObligatory, parameters.IsObligatory);
        }

        [Fact]
        public void Can_Map_Update_Dto()
        {
            var dto = new GlobalOptionUpdateRequestDTO()
            {
                Description = A<string>(),
                IsObligatory = A<bool>(),
                IsEnabled = A<bool>(),
                Name = A<string>(),
            };
            _httpRequest.Setup(x =>
                    x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetAllInputPropertyNames<GlobalOptionUpdateRequestDTO>());

            var parameters = _sut.ToGlobalOptionUpdateParameters(dto);

            Assert.Equal(dto.Description, parameters.Description.NewValue.Value);

        }
    }
}
