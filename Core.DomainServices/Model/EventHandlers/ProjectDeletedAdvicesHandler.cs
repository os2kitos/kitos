using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainModel.ItProject;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainServices.Model.EventHandlers
{
    public class ProjectDeletedAdvicesHandler : IDomainEventHandler<EntityDeletedEvent<ItProject>>
    {
        private readonly IGenericRepository<Advice> _adviceRepository;

        public ProjectDeletedAdvicesHandler(IGenericRepository<Advice> adviceRepository)
        {
            _adviceRepository = adviceRepository;
        }

        public void Handle(EntityDeletedEvent<ItProject> domainEvent)
        {
            var projectDeleted = domainEvent.Entity;
            var toBeDeleted = _adviceRepository.Get(a => a.RelationId == projectDeleted.Id && a.Type == ObjectType.itProject).ToList();
            foreach (var advice in toBeDeleted)
            {
                _adviceRepository.DeleteByKeyWithReferencePreload(advice.Id);
            }
        }
    }
}
