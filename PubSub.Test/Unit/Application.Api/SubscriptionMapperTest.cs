using PubSub.Application.Api.DTOs.Request;
using PubSub.Application.Api.Mapping;
using PubSub.Core.DomainModel.Subscriptions;
using PubSub.Test.Base.Tests.Toolkit.Patterns;

namespace PubSub.Test.Unit.Application.Api
{
    public class SubscriptionMapperTest : WithAutoFixture
    {
        private SubscriptionMapper _sut;

        public SubscriptionMapperTest()
        {
            _sut = new SubscriptionMapper();
        }

        [Fact]
        public void Can_Map_From_Dto()
        {
            var dto = A<SubscribeRequestDto>();

            var createSubscriptionRequests = _sut.FromDTO(dto).ToList();

            Assert.All(createSubscriptionRequests.Select(x => x.Callback), requestCallback => Assert.Equal(dto.Callback.AbsoluteUri, requestCallback));
            var createRequestSet = createSubscriptionRequests.Select(x => x.Topic).ToHashSet();
            var dtoSet = dto.Topics.ToHashSet();
            Assert.True(createRequestSet.SetEquals(dtoSet));
        }

        [Fact]
        public void Can_Map_To_Dto()
        {
            var subscription = A<Subscription>();

            var dto = _sut.ToResponseDTO(subscription);

            Assert.Equal(subscription.Topic, dto.Topic);
            Assert.Equal(subscription.Uuid, dto.Uuid);
            Assert.Equal(subscription.Callback, subscription.Callback);

        }
    }
}
