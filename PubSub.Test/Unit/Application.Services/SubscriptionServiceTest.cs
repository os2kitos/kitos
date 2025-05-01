using CSharpFunctionalExtensions;
using Moq;
using PubSub.Application.Services;
using PubSub.Application.Services.CurrentUserService;
using PubSub.Core.Abstractions.ErrorTypes;
using PubSub.Core.DomainModel.Parameters;
using PubSub.Core.DomainModel.Repositories;
using PubSub.Core.DomainModel.Subscriptions;
using PubSub.Core.DomainServices;
using PubSub.Test.Base.Tests.Toolkit.Patterns;

namespace PubSub.Test.Unit.Application.Services
{
    public class SubscriptionServiceTest : WithAutoFixture
    {
        private readonly Mock<ISubscriptionRepository> _repository = new Mock<ISubscriptionRepository>();
        private readonly Mock<ICurrentUserService> _currentUserService = new Mock<ICurrentUserService>();
        private readonly Mock<ITopicConsumerInstantiatorService> _consumerInstantiator = new Mock<ITopicConsumerInstantiatorService>();
        private readonly SubscriptionService _sut;

        public SubscriptionServiceTest()
        {
            _sut = new SubscriptionService(_repository.Object, _currentUserService.Object,
                _consumerInstantiator.Object);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async void Can_Only_Delete_Subscription_If_User_Owns_It(bool userOwnsSubscription)
        {
            var currentUserId = ExpectCurrentUserIs();
            var subscriptionOwnerId = userOwnsSubscription ? currentUserId : A<string>();
            var subscription = CreateSubscription(subscriptionOwnerId);
            _repository.Setup(x => x.GetAsync(subscription.Uuid)).ReturnsAsync(subscription);

            var result = await _sut.DeleteSubscription(subscription.Uuid);

            var expectedResult = userOwnsSubscription ? Maybe.None : Maybe.From(OperationError.Forbidden);
            var expectedDeleteCalls = userOwnsSubscription ? Times.Once() : Times.Never();
            Assert.Equal(expectedResult, result);
            _repository.Verify(x => x.DeleteAsync(subscription), expectedDeleteCalls);
        }

        [Fact]
        public async void Can_Delete_Returns_Not_Found_When_Subscription_Does_Not_Exist()
        {
            var uuid = A<Guid>();
            _repository.Setup(x => x.GetAsync(uuid)).ReturnsAsync(Maybe.None);

            var result = await _sut.DeleteSubscription(uuid);

            Assert.Equal(OperationError.NotFound, result);
        }

        [Fact]
        public async void Does_Not_Create_Subscriptions_For_Duplicates()
        {
            var request = A<CreateSubscriptionParameters>();
            ExpectCurrentUserIs();
            _repository.Setup(x => x.Exists(request.Topic, request.Callback)).ReturnsAsync(true);

            await _sut.AddSubscriptionsAsync(new List<CreateSubscriptionParameters> { request });

            _repository.Verify(x => x.AddAsync(It.IsAny<Subscription>()), Times.Never);
            _consumerInstantiator.Verify(x => x.InstantiateTopic(request.Topic), Times.Never);
        }

        [Fact]
        public async Task Can_Add_Subscriptions_With_Correct_Values()
        {
            //Arrange
            var subscriptionRequests = Many<CreateSubscriptionParameters>().ToList();
            var currentUserId = ExpectCurrentUserIs();

            var addedSubscriptions = new List<Subscription>();
            _repository.Setup(x => x.AddAsync(It.IsAny<Subscription>()))
                .Callback<Subscription>(addedSubscriptions.Add)
                .Returns(Task.CompletedTask);

            //Act
            await _sut.AddSubscriptionsAsync(subscriptionRequests);

            //Assert
            Assert.Equal(subscriptionRequests.Count, addedSubscriptions.Count);

            foreach (var request in subscriptionRequests)
            {
                var subscription = addedSubscriptions
                    .FirstOrDefault(s => s.Callback == request.Callback && s.Topic == request.Topic);

                Assert.NotNull(subscription); // Ensure a matching subscription was created.
                Assert.Equal(currentUserId, subscription.OwnerId);
                Assert.NotEqual(Guid.Empty, subscription.Uuid);
            }

            foreach (var subscription in addedSubscriptions)
            {
                _consumerInstantiator.Verify(x => x.InstantiateTopic(subscription.Topic), Times.Once());
            }
        }

        private string ExpectCurrentUserIs()
        {
            var userId = A<string>();
            _currentUserService.Setup(x => x.UserId).Returns(Maybe<string>.From(userId));
            return userId;
        }

        private IEnumerable<Subscription> CreateSubscriptions(string currentUserId)
        {
            var someOtherUserId = A<string>();
            var s1 = CreateSubscription(currentUserId);
            var s2 = CreateSubscription(currentUserId);
            var s3 = CreateSubscription(someOtherUserId);
            return new List<Subscription> { s1, s2, s3 };
        }

        private Subscription CreateSubscription(string ownerId)
        {
            return new Subscription(A<string>(), A<string>(), ownerId);
        }
    }
}
