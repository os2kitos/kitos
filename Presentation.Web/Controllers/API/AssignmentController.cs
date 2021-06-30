using System.Linq;
using Core.DomainModel.Constants;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.Project;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class AssignmentController : GenericApiController<Assignment, AssignmentDTO>
    {
        private readonly IItProjectRepository _projectRepository;

        public AssignmentController(
            IGenericRepository<Assignment> repository,
            IItProjectRepository projectRepository)
            : base(repository)
        {
            _projectRepository = projectRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<Assignment, ItProject>(x => _projectRepository.GetById(x.AssociatedItProjectId.GetValueOrDefault(EntityConstants.InvalidId)), base.GetCrudAuthorization());
        }

        protected override IQueryable<Assignment> GetAllQuery()
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
