using Microsoft.EntityFrameworkCore;
using PubSub.Infrastructure.DataAccess.Repositories;
using PubSub.Infrastructure.DataAccess;
using PubSub.Test.Base.Tests.Toolkit.Patterns;
using PubSub.Core.DomainModel.Subscriptions;

namespace PubSub.Test.Unit.Infrastructure.DataAccess;

public class EntityFrameworkSubscriptionRepositoryTests : WithAutoFixture
{
    private DbContextOptions<PubSubContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<PubSubContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Add_Async_Should_Add_Subscription_To_Database()
    {
        var options = CreateNewContextOptions();
        var subscription = A<Subscription>();

        await using (var context = new PubSubContext(options))
        {
            var repository = new EntityFrameworkSubscriptionRepository(context);
            await repository.AddAsync(subscription);
        }

        await using (var context = new PubSubContext(options))
        {
            var repository = new EntityFrameworkSubscriptionRepository(context);
            var retrieved = await repository.GetAsync(subscription.Uuid);
            Assert.True(retrieved.HasValue);
            Assert.Equal(subscription.Callback, retrieved.Value.Callback);
            Assert.Equal(subscription.Topic, retrieved.Value.Topic);
            Assert.Equal(subscription.OwnerId, retrieved.Value.OwnerId);
        }
    }

    [Fact]
    public async Task Exists_Should_Return_True_When_Subscription_Exists()
    {
        var options = CreateNewContextOptions();
        var topic = A<string>();
        var callback = A<string>();
        await using (var context = new PubSubContext(options))
        {
            var subscription = new Subscription(callback, topic, A<string>());
            context.Subscriptions.Add(subscription);
            await context.SaveChangesAsync();
        }

        await using (var context = new PubSubContext(options))
        {
            var repository = new EntityFrameworkSubscriptionRepository(context);
            var exists = await repository.Exists(topic, callback);
            Assert.True(exists);
        }
    }

    [Fact]
    public async Task Exists_Should_Return_False_When_Subscription_Does_Not_Exist()
    {
        var options = CreateNewContextOptions();

        await using var context = new PubSubContext(options);
        var repository = new EntityFrameworkSubscriptionRepository(context);
        var exists = await repository.Exists("NonExistentTopic", "http://nonexistent.url");
        Assert.False(exists);
    }

    [Fact]
    public async Task GetAll_ByUser_Id_Should_Return_Only_Subscriptions_For_Given_User()
    {
        var options = CreateNewContextOptions();
        var userId = "User1";
        await using (var context = new PubSubContext(options))
        {
            context.Subscriptions.AddRange(new List<Subscription>
            {
                new Subscription("http://callback1.url", "Topic1", userId),
                new Subscription("http://callback2.url", "Topic2", userId),
                new Subscription("http://callback3.url", "Topic3", "OtherUser")
            });
            await context.SaveChangesAsync();
        }

        await using (var context = new PubSubContext(options))
        {
            var repository = new EntityFrameworkSubscriptionRepository(context);
            var userSubscriptions = (await repository.GetAllByUserId(userId)).ToList();
            Assert.Equal(2, userSubscriptions.Count);
            foreach (var subscription in userSubscriptions)
            {
                Assert.Equal(userId, subscription.OwnerId);
            }
        }
    }

    [Fact]
    public async Task Get_By_Topic_Should_Return_Only_Subscriptions_For_Given_Topic()
    {
        var options = CreateNewContextOptions();
        var topic = "SpecificTopic";
        await using (var context = new PubSubContext(options))
        {
            context.Subscriptions.AddRange(new List<Subscription>
            {
                new Subscription("http://callback1.url", topic, "User1"),
                new Subscription("http://callback2.url", topic, "User2"),
                new Subscription("http://callback3.url", "OtherTopic", "User3")
            });
            await context.SaveChangesAsync();
        }

        await using (var context = new PubSubContext(options))
        {
            var repository = new EntityFrameworkSubscriptionRepository(context);
            var topicSubscriptions = (await repository.GetByTopic(topic)).ToList();
            Assert.Equal(2, topicSubscriptions.Count);
            foreach (var subscription in topicSubscriptions)
            {
                Assert.Equal(topic, subscription.Topic);
            }
        }
    }

    [Fact]
    public async Task Delete_Async_Should_Remove_Subscription_From_Database()
    {
        var options = CreateNewContextOptions();
        Subscription subscription;
        await using (var context = new PubSubContext(options))
        {
            subscription = new Subscription("http://callback.url", "TestTopic", "User1");
            context.Subscriptions.Add(subscription);
            await context.SaveChangesAsync();
        }

        await using (var context = new PubSubContext(options))
        {
            var repository = new EntityFrameworkSubscriptionRepository(context);
            await repository.DeleteAsync(subscription);
        }

        
        await using (var context = new PubSubContext(options))
        {
            var repository = new EntityFrameworkSubscriptionRepository(context);
            var retrieved = await repository.GetAsync(subscription.Uuid);
            Assert.False(retrieved.HasValue);
        }
    }
}