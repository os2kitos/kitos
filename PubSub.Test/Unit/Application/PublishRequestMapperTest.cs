using AutoFixture;
using PubSub.Application.DTOs;
using PubSub.Application.Mapping;
using PubSub.Test.Base.Tests.Toolkit.Patterns;
using System.Text.Json;

namespace PubSub.Test.Unit.Application
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
