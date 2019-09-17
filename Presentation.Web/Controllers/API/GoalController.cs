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
    public class GoalController : GenericApiController<Goal, GoalDTO>
    {
        private readonly IItProjectRepository _projectRepository;
        private readonly IGenericRepository<GoalStatus> _goalStatusRepository;

        public GoalController(
            IGenericRepository<Goal> repository, 
            IAuthorizationContext authorization, 
            IItProjectRepository projectRepository,
            IGenericRepository<GoalStatus> goalStatusRepository) 
            : base(repository, authorization)
        {
            _projectRepository = projectRepository;
            _goalStatusRepository = goalStatusRepository;
        }

        protected override bool AllowCreate<T>(IEntity entity)
        {
            if (entity is Goal relation)
            {
                var goalStatus = _goalStatusRepository.GetByKey(relation.GoalStatusId);
                var project = _projectRepository.GetById(goalStatus.ItProject.Id);
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
            if (entity is Goal relation)
            {
                return condition.Invoke(relation.GoalStatus.ItProject);
            }

            return false;
        }
    }
}
