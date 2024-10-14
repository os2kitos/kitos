
using Moq;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2.Internal
{
    public class LocalOptionTypeWriteModelMapperTest: WithAutoFixture
    {
        [Fact]
        public void LocalOptionCreateParameters_Should_Map_To_LocalRegularOptionCreateRequestDTO()
        {
            var dto = new LocalRegularOptionCreateRequestDTO
            {
                OptionId = A<int>()
            };
            var sut = new LocalOptionTypeWriteModelMapper(new Mock<ICurrentHttpRequest>().Object);

            var parameters = sut.ToLocalOptionCreateParameters(dto);

            Assert.Equal(dto.OptionId, parameters.OptionId);
        }
    }
}
