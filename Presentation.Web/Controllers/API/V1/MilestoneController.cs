using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Constants;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.Project;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models;
using Presentation.Web.Models.API.V1;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    public class MilestoneController : GenericApiController<Milestone, MilestoneDTO>
    {
        private readonly IItProjectRepository _projectRepository;

        public MilestoneController(IGenericRepository<Milestone> repository, IItProjectRepository projectRepository)
            : base(repository)
        {
            _projectRepository = projectRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<Milestone, ItProject>(x => _projectRepository.GetById(x.AssociatedItProjectId.GetValueOrDefault(EntityConstants.InvalidId)), base.GetCrudAuthorization());
        }

        protected override IQueryable<Milestone> GetAllQuery()
        {
            var query = Repository.AsQueryable();
            if (AuthorizationContext.GetCrossOrganizationReadAccess() == CrossOrganizationDataReadAccessLevel.All)
            {
                return query;
            }

            var organizationIds = UserContext.OrganizationIds.ToList();

            return query.Where(x => x.AssociatedItProject != null && organizationIds.Contains(x.AssociatedItProject.OrganizationId));
        }
    }
}
