using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DomainEvents;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Repositories.KLE;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainModel.ItSystemUsage.Handlers
{
    public class ExposingSystemChangedHandler: IDomainEventHandler<ExposingSystemChanged>
    {
        private readonly IGenericRepository<ItSystemUsage> _systemUsageRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IOrganizationalUserContext _userContext;
        private readonly IOperationClock _clock;

        public ExposingSystemChangedHandler(IGenericRepository<ItSystemUsage> systemUsageRepository,
            ITransactionManager transactionManager, IOrganizationalUserContext userContext, IOperationClock clock)
        {
            _systemUsageRepository = systemUsageRepository;
            _transactionManager = transactionManager;
            _userContext = userContext;
            _clock = clock;
        }

        public void Handle(ExposingSystemChanged @event)
        {
            using (var transaction = _transactionManager.Begin(IsolationLevel.Serializable))
            {
                var updateTime = _clock.Now;

                foreach (var systemRelation in @event.Interface.AssociatedSystemRelations)
                {
                    var withReferencePreload = _systemUsageRepository
                        .GetWithReferencePreload(su => su.UsageRelations);

                    var systemUsageWithRelation = withReferencePreload
                        .First(su => su.UsageRelations.Contains(systemRelation));

                    systemUsageWithRelation.ModifyUsageRelation(_userContext.UserEntity, systemRelation.Id, systemRelation.ToSystemUsage, 
                        systemRelation.Description, systemRelation.Reference, 
                        null, 
                        Maybe<ItContract.ItContract>.None, 
                        Maybe<RelationFrequencyType>.None);
                    systemUsageWithRelation.LastChangedByUser = _userContext.UserEntity;
                    systemUsageWithRelation.LastChanged = updateTime;
                }

                _systemUsageRepository.Save();

                transaction.Commit();
            }
        }
    }
}
