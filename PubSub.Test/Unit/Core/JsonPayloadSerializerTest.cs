using System.Text;
using System.Text.Json;
using PubSub.Core.Services.Serializer;
using PubSub.Test.Base.Tests.Toolkit.Patterns;

namespace PubSub.Test.Unit.Core
{
    public class JsonPayloadSerializerTest : WithAutoFixture
    {
        private readonly JsonPayloadSerializer _sut;

        public JsonPayloadSerializerTest()
        {
            _sut = new JsonPayloadSerializer();
        }

        [Fact]
        public void Can_Serialize()
        {
            var jsonString = "\"Hello, World!\"";
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString);

            var result = _sut.Serialize(jsonElement);

            var expected = Encoding.UTF8.GetBytes(jsonString);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Can_Deserialize()
        {
            var jsonString = "{\"Name\":\"John\",\"Age\":30}";
            var bytes = Encoding.UTF8.GetBytes(jsonString);

            var result = _sut.Deserialize(bytes);

            Assert.Equal("John", result.GetProperty("Name").GetString());
            Assert.Equal(30, result.GetProperty("Age").GetInt32());
        }
    }
}