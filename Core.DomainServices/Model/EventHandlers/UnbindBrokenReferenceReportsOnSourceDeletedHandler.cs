using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Qa.References;
using Infrastructure.Services.DataAccess;

namespace Core.DomainServices.Model.EventHandlers
{
    public class UnbindBrokenReferenceReportsOnSourceDeletedHandler :
        IDomainEventHandler<EntityBeingDeletedEvent<ItInterface>>,
        IDomainEventHandler<EntityBeingDeletedEvent<ExternalReference>>
    {
        private readonly IGenericRepository<BrokenLinkInExternalReference> _externalReferenceBrokenLinks;
        private readonly IGenericRepository<BrokenLinkInInterface> _interfaceBrokenLinks;
        private readonly ITransactionManager _transactionManager;

        public UnbindBrokenReferenceReportsOnSourceDeletedHandler(
            IGenericRepository<BrokenLinkInExternalReference> externalReferenceBrokenLinks,
            IGenericRepository<BrokenLinkInInterface> interfaceBrokenLinks,
            ITransactionManager transactionManager)
        {
            _externalReferenceBrokenLinks = externalReferenceBrokenLinks;
            _interfaceBrokenLinks = interfaceBrokenLinks;
            _transactionManager = transactionManager;
        }

        public void Handle(EntityBeingDeletedEvent<ExternalReference> domainEvent)
        {
            using var transaction = _transactionManager.Begin();
            foreach (var report in domainEvent.Entity.BrokenLinkReports.ToList())
            {
                _externalReferenceBrokenLinks.Delete(report);
            }
            transaction.Commit();
        }

        public void Handle(EntityBeingDeletedEvent<ItInterface> domainEvent)
        {
            using var transaction = _transactionManager.Begin();
            foreach (var report in domainEvent.Entity.BrokenLinkReports.ToList())
            {
                _interfaceBrokenLinks.Delete(report);
            }
            transaction.Commit();
        }
    }
}
