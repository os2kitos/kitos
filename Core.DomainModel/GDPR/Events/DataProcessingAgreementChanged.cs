using Infrastructure.Services.DomainEvents;

namespace Core.DomainModel.GDPR.Events
{
    public class DataProcessingAgreementChanged : IDomainEvent
    {
        public enum ChangeType
        {
            Created,
            Updated,
            Deleted
        }
        public DataProcessingAgreement DataProcessingAgreement { get; }
        public ChangeType Change { get; }

        public DataProcessingAgreementChanged(DataProcessingAgreement dataProcessingAgreement, ChangeType change)
        {
            DataProcessingAgreement = dataProcessingAgreement;
            Change = change;
        }
    }
}
