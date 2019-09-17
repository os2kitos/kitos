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
    public class AssignmentController : GenericContextAwareApiController<Assignment, AssignmentDTO>
    {
        private readonly IItProjectRepository _projectRepository;

        public AssignmentController(IGenericRepository<Assignment> repository, IItProjectRepository projectRepository, IAuthorizationContext authorizationContext)
            : base(repository, authorizationContext)
        {
            _projectRepository = projectRepository;
        }

        protected override bool AllowCreate<T>(IEntity entity)
        {
            if (entity is Assignment relation)
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
            if (entity is Assignment relation)
            {
                return condition.Invoke(relation.AssociatedItProject);
            }

            return false;
        }
    }
}
