using AutoFixture;
using PubSub.Application.DTOs;
using PubSub.Application.Mapping;
using PubSub.Test.Base.Tests.Toolkit.Patterns;

namespace PubSub.Test.Unit.Application
{
    public class SubscribeRequestMapperTest: WithAutoFixture
    {
        private SubscribeRequestMapper _sut;

        public SubscribeRequestMapperTest()
        {
            _sut = new SubscribeRequestMapper();
        }

        [Fact]
        public void Can_Map_From_Dto()
        {
            var dto = A<SubscribeRequestDto>();

            var subscription = _sut.FromDto(dto);

            Assert.Equal(dto.Callback, subscription.Callback);
            Assert.Equal(dto.Topics, subscription.Topics.Select(t => t.Name));
        }
    }
}
