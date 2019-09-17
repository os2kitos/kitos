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
    public class StakeholderController : GenericContextAwareApiController<Stakeholder, StakeholderDTO>
    {
        private readonly IItProjectRepository _projectRepository;

        public StakeholderController(
            IGenericRepository<Stakeholder> repository,
            IAuthorizationContext authorization,
            IItProjectRepository projectRepository)
            : base(repository, authorization)
        {
            _projectRepository = projectRepository;
        }

        protected override bool AllowCreate<T>(IEntity entity)
        {
            if (entity is Stakeholder relation)
            {
                var project = _projectRepository.GetById(relation.ItProjectId);
                return project != null && base.AllowModify(project);
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
            if (entity is Stakeholder relation)
            {
                return condition.Invoke(relation.ItProject);
            }

            return false;
        }
    }
}
