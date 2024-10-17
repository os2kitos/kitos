
using System;
using System.Linq;
using Moq;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2.Internal
{
    public class LocalOptionTypeWriteModelMapperTest: WriteModelMapperTestBase
    {
        private readonly LocalOptionTypeWriteModelMapper _sut;
        private readonly Mock<ICurrentHttpRequest> _httpRequest;

        public LocalOptionTypeWriteModelMapperTest()
        {
            _httpRequest = new Mock<ICurrentHttpRequest>();
            _sut = new LocalOptionTypeWriteModelMapper(_httpRequest.Object);
        }

        [Fact]
        public void Can_Map_Create_Dto()
        {
            var dto = new LocalOptionCreateRequestDTO
            {
                OptionUuid = A<Guid>()
            };
            _httpRequest.Setup(x =>
                    x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetAllInputPropertyNames<LocalOptionCreateRequestDTO>());

            var parameters = _sut.ToLocalOptionCreateParameters(dto);

            Assert.Equal(dto.OptionUuid, parameters.OptionUuid);
        }

        [Fact]
        public void Can_Map_Update_Dto()
        {
            var dto = new LocalRegularOptionUpdateRequestDTO()
            {
                Description = A<string>()
            };
            _httpRequest.Setup(x =>
                    x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetAllInputPropertyNames<LocalRegularOptionUpdateRequestDTO>());

            var parameters = _sut.ToLocalOptionUpdateParameters(dto);

            Assert.Equal(dto.Description, parameters.Description.NewValue.Value);
        }
    }
}
