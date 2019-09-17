using System;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Repositories.Project;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class MilestoneController : GenericContextAwareApiController<Milestone, MilestoneDTO>
    {
        private readonly IItProjectRepository _projectRepository;

        public MilestoneController(
            IGenericRepository<Milestone> repository,
            IAuthorizationContext authorization,
            IItProjectRepository projectRepository)
            : base(repository, authorization)
        {
            _projectRepository = projectRepository;
        }

        protected override bool AllowCreate<T>(IEntity entity)
        {
            if (entity is Milestone relation)
            {
                if (relation.AssociatedItProjectId.HasValue)
                {
                    var project = _projectRepository.GetById(relation.AssociatedItProjectId.Value);
                    return project != null && base.AllowModify(project);
                }
            }
            return false;
        }

        protected override bool AllowModify(IEntity entity)
        {
            return GeAuthorizationFromRoot(entity, base.AllowModify);
        }

        protected override bool AllowDelete(IEntity entity)
        {
            //Check if modification, not deletion, of parent usage (the root aggregate) is allowed 
            return GeAuthorizationFromRoot(entity, base.AllowModify);
        }

        protected override bool AllowRead(IEntity entity)
        {
            return GeAuthorizationFromRoot(entity, base.AllowRead);
        }

        private static bool GeAuthorizationFromRoot(IEntity entity, Predicate<ItProject> condition)
        {
            if (entity is Milestone relation)
            {
                return condition.Invoke(relation.AssociatedItProject);
            }

            return false;
        }
    }
}
