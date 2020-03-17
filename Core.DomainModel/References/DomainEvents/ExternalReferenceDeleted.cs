using Infrastructure.Services.DomainEvents;

namespace Core.DomainModel.References.DomainEvents
{
    public class ExternalReferenceDeleted : IDomainEvent
    {
        public ExternalReference DeletedExternalReference { get; }

        public ExternalReferenceDeleted(ExternalReference deletedExternalReference)
        {
            DeletedExternalReference = deletedExternalReference;
        }
    }
}
