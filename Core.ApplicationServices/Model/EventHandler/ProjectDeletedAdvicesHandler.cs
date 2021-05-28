using System.Linq;
using Core.DomainModel.Advice;
using Core.DomainModel.ItProject;
using Core.DomainServices.Repositories.Advice;
using Infrastructure.Services.DomainEvents;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class ProjectDeletedAdvicesHandler : IDomainEventHandler<EntityDeletedEvent<ItProject>>
    {
        private readonly IAdviceRepository _adviceRepository;
        private readonly IAdviceService _adviceService;

        public ProjectDeletedAdvicesHandler(IAdviceRepository adviceRepository, IAdviceService adviceService)
        {
            _adviceRepository = adviceRepository;
            _adviceService = adviceService;
        }

        public void Handle(EntityDeletedEvent<ItProject> domainEvent)
        {
            var projectDeleted = domainEvent.Entity;
            var toBeDeleted = _adviceRepository.GetByRelationIdAndType(projectDeleted.Id, ObjectType.itProject).ToList();
            _adviceService.BulkDeleteAdvice(toBeDeleted);
        }
    }
}
