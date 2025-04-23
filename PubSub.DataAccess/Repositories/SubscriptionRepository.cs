using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using PubSub.Core.Models;

namespace PubSub.DataAccess.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly PubSubContext _context;
    public SubscriptionRepository(PubSubContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Subscription>> GetAllByUserId(string userId)
    {
        return await AsQueryable().Where(x => x.OwnerId == userId).ToListAsync();
    }

    public async Task<IEnumerable<Subscription>> GetByTopic(string topic)
    {
        return await SubscriptionsByTopic(topic).ToListAsync();
    }

    public async Task<bool> Exists(string topic, string url)
    {
        return await SubscriptionsByTopic(topic).AnyAsync(x => x.Callback == url);
    }

    public async Task<Maybe<Subscription>> GetAsync(Guid uuid)
    {
        var subscription = await AsQueryable().FirstOrDefaultAsync(x => x.Uuid == uuid);
        return Maybe<Subscription>.From(subscription);
    }


    public async Task AddAsync(Subscription subscription)
    {
        await _context.Subscriptions.AddAsync(subscription);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Subscription subscription)
    {
        _context.Subscriptions.Remove(subscription);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Subscription> AsQueryable()
    {
        return _context.Subscriptions;
    }

    private IQueryable<Subscription> SubscriptionsByTopic(string topic)
    {
        return AsQueryable().Where(x => x.Topic == topic);
    }
}