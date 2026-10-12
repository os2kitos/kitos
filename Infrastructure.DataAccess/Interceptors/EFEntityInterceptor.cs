using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainServices.Context;
using Core.DomainServices.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.DataAccess.Interceptors
{
    public class EFEntityInterceptor : SaveChangesInterceptor
    {
        private readonly Factory<IOperationClock> _operationClock;
        private readonly Factory<Maybe<ActiveUserIdContext>> _userContext;
        private readonly Factory<IFallbackUserResolver> _fallbackUserResolver;

        public EFEntityInterceptor(
            Factory<IOperationClock> operationClock,
            Factory<Maybe<ActiveUserIdContext>> userContext,
            Factory<IFallbackUserResolver> fallbackUserResolver)
        {
            _operationClock = operationClock;
            _userContext = userContext;
            _fallbackUserResolver = fallbackUserResolver;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context != null)
                ApplyAuditFields(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context != null)
                ApplyAuditFields(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ApplyAuditFields(DbContext context)
        {
            var userId = GetActiveUserId();
            var now = _operationClock().Now;

            foreach (var entry in context.ChangeTracker.Entries<IEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.ObjectOwnerId = userId;
                        entry.Entity.LastChangedByUserId = userId;
                        entry.Entity.LastChanged = now;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastChangedByUserId = userId;
                        entry.Entity.LastChanged = now;
                        if (entry.Entity.ObjectOwnerId == 0)
                            entry.Entity.ObjectOwnerId = userId;
                        break;
                }
            }
        }

        private int GetActiveUserId()
        {
            var userContext = _userContext();
            if (userContext.HasValue)
            {
                return userContext.Value.ActiveUserId;
            }
            return _fallbackUserResolver().Resolve().Id;
        }
    }
}
