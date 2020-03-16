using System.Data;
using Core.DomainModel.ItSystem.DomainEvents;
using Core.DomainModel.Qa.References;
using Core.DomainModel.References.DomainEvents;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainServices.Model.EventHandlers
{
    public class UnbindBrokenReferenceReportsOnSourceDeletedHandler :
        IDomainEventHandler<InterfaceDeleted>,
        IDomainEventHandler<ExternalReferenceDeleted>
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

        public void Handle(InterfaceDeleted domainEvent)
        {
            using (var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted))
            {
                foreach (var report in domainEvent.DeletedInterface.BrokenLinkReports)
                {
                    _interfaceBrokenLinks.Delete(report);
                }
                transaction.Commit();
            }
        }

        public void Handle(ExternalReferenceDeleted domainEvent)
        {
            using (var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted))
            {
                foreach (var report in domainEvent.DeletedExternalReference.BrokenLinkReports)
                {
                    _externalReferenceBrokenLinks.Delete(report);
                }
                transaction.Commit();
            }
        }
    }
}
