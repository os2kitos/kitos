using System.Linq;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.Project;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
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
            IItProjectRepository projectRepository,
            IGenericRepository<GoalStatus> goalStatusRepository)
            : base(repository)
        {
            _projectRepository = projectRepository;
            _goalStatusRepository = goalStatusRepository;
        }

        private ItProject GetItProject(Goal relation)
        {
            var goalStatus = _goalStatusRepository.GetByKey(relation.GoalStatusId);
            if (goalStatus != null)
            {
                var project = _projectRepository.GetById(goalStatus.ItProject.Id);
                return project;
            }

            return default(ItProject);
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<Goal, ItProject>(GetItProject, base.GetCrudAuthorization());
        }

        protected override IQueryable<Goal> GetAllQuery()
        {
            var query = Repository.AsQueryable();
            if (AuthorizationContext.GetCrossOrganizationReadAccess() == CrossOrganizationDataReadAccessLevel.All)
            {
                return query;
            }

            var organizationIds = UserContext.OrganizationIds.ToList();

            return query.Where(x => organizationIds.Contains(x.GoalStatus.ItProject.OrganizationId));
        }
    }
}
