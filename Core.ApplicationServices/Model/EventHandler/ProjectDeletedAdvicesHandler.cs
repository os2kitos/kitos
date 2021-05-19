using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Infrastructure.Services.DomainEvents;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class ProjectDeletedAdvicesHandler : IDomainEventHandler<EntityDeletedEvent<ItProject>>
    {
        private readonly IGenericRepository<Advice> _adviceRepository;
        private readonly IAdviceService _adviceService;

        public ProjectDeletedAdvicesHandler(IGenericRepository<Advice> adviceRepository, IAdviceService adviceService)
        {
            _adviceRepository = adviceRepository;
            _adviceService = adviceService;
        }

        public void Handle(EntityDeletedEvent<ItProject> domainEvent)
        {
            var projectDeleted = domainEvent.Entity;
            var toBeDeleted = _adviceRepository.Get(a => a.RelationId == projectDeleted.Id && a.Type == ObjectType.itProject).ToList();
            _adviceService.BulkDeleteAdvice(toBeDeleted);
        }
    }
}
