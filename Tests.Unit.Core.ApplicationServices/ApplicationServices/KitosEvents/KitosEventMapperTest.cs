using Newtonsoft.Json;
using Xunit;
using Core.ApplicationServices.Model.KitosEvents;
using Tests.Toolkit.Patterns;

namespace Tests.Unit.Core.ApplicationServices.KitosEvents
{
    public class KitosEventMapperTest : WithAutoFixture
    {
        private readonly KitosEventMapper _sut;

        public KitosEventMapperTest()
        {
            _sut = new KitosEventMapper();
        }

        [Fact]
        public void Can_Map_Kitos_Events()
        {
            var eventBody = A<SystemChangeEventBodyModel>();
            var expectedMessage = JsonConvert.SerializeObject(eventBody.ToKeyValuePairs());
            var expectedTopic = A<string>();
            var kitosEvent = new KitosEvent(eventBody, expectedTopic);

            var dto = _sut.MapKitosEventToDTO(kitosEvent);

            Assert.Equal(expectedMessage, dto.Message);
            Assert.Equal(expectedTopic, dto.Topic);
        }
    }
}