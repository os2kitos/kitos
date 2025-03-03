using PubSub.Core.Services.Serializer;
using PubSub.Test.Base.Tests.Toolkit.Patterns;
using System.Text;

namespace PubSub.Test.Unit.Core
{
    public class UTF8MessageSerializerTest: WithAutoFixture
    {
        private UTF8MessageSerializer _sut;

        public UTF8MessageSerializerTest() {
            _sut = new UTF8MessageSerializer();
        }

        [Fact]
        public void Can_Serialize()
        {

            var message = A<string>();

            var result = _sut.Serialize(message);

            var expected = Encoding.UTF8.GetBytes(message);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Can_Deserialize()
        {

            var bytes = A<byte[]>();
            var expected = Encoding.UTF8.GetString(bytes);

            var result = _sut.Deserialize(bytes);

            Assert.Equal(expected, result);
        }
    }
}
