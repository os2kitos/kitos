using PubSub.Application.Api.Mapping;
using PubSub.Application.Api.DTOs.Request;
using PubSub.Test.Base.Tests.Toolkit.Patterns;
using System.Text.Json;

namespace PubSub.Test.Unit.Application.Api
{
    public class PublishRequestMapperTest : WithAutoFixture
    {
        private PublishRequestMapper _sut;

        public PublishRequestMapperTest()
        {
            _sut = new PublishRequestMapper();
        }

        [Fact]
        public void Can_Map_From_Dto()
        {
            var jsonString = "\"Hello, World!\"";
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString);
            var dto = new PublishRequestDto { Topic = A<string>(), Payload = jsonElement };

            var publication = _sut.FromDto(dto);

            Assert.Equal(dto.Topic, publication.Topic.Name);
            Assert.Equal(dto.Payload, publication.Payload);
        }
    }
}
