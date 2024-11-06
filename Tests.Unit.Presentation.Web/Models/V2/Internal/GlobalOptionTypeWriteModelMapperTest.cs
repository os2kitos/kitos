
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using System.Linq;
using Xunit;
using Moq;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Internal.Request;
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
        public void Can_Map_Regular_Create_Dto()
        {
            var dto = A<GlobalRegularOptionCreateRequestDTO>();

            var parameters = _sut.ToGlobalRegularOptionCreateParameters(dto);

            Assert.Equal(dto.Name, parameters.Name);
            Assert.Equal(dto.Description, parameters.Description);
            Assert.Equal(dto.IsObligatory, parameters.IsObligatory);
        }

        [Fact]
        public void Can_Map_Regular_Update_Dto()
        {
            var dto = A<GlobalRegularOptionUpdateRequestDTO>();
            _httpRequest.Setup(x =>
                    x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetAllInputPropertyNames<GlobalRegularOptionUpdateRequestDTO>());

            var parameters = _sut.ToGlobalRegularOptionUpdateParameters(dto);

            Assert.Equal(dto.Description, parameters.Description.NewValue.Value);
            Assert.Equal(dto.IsObligatory, parameters.IsObligatory.NewValue.Value);
            Assert.Equal(dto.IsEnabled, parameters.IsEnabled.NewValue.Value);
            Assert.Equal(dto.Name, parameters.Name.NewValue.Value);
            Assert.Equal(dto.Priority, parameters.Priority.NewValue.Value);
        }

        [Fact]
        public void Can_Map_Role_Create_Dto()
        {
            var dto = A<GlobalRoleOptionCreateRequestDTO>();

            var parameters = _sut.ToGlobalRoleOptionCreateParameters(dto);

            Assert.Equal(dto.Name, parameters.Name);
            Assert.Equal(dto.Description, parameters.Description);
            Assert.Equal(dto.IsObligatory, parameters.IsObligatory);
            Assert.Equal(dto.WriteAccess, parameters.WriteAccess);
        }

        [Fact]
        public void Can_Map_Role_Update_Dto()
        {
            var dto = A<GlobalRoleOptionUpdateRequestDTO>();
            _httpRequest.Setup(x =>
                    x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetAllInputPropertyNames<GlobalRoleOptionUpdateRequestDTO>());

            var parameters = _sut.ToGlobalRoleOptionUpdateParameters(dto);

            Assert.Equal(dto.Description, parameters.Description.NewValue.Value);
            Assert.Equal(dto.IsObligatory, parameters.IsObligatory.NewValue.Value);
            Assert.Equal(dto.IsEnabled, parameters.IsEnabled.NewValue.Value);
            Assert.Equal(dto.Name, parameters.Name.NewValue.Value);
            Assert.Equal(dto.WriteAccess, parameters.WriteAccess.NewValue.Value);
            Assert.Equal(dto.Priority, parameters.Priority.NewValue.Value);
        }
    }
}
