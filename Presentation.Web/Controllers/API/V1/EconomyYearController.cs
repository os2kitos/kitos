using System.Linq;
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
    public class EconomyYearController : GenericApiController<EconomyYear, EconomyYearDTO>
    {
        private readonly IItProjectRepository _projectRepository;

        public EconomyYearController(
            IGenericRepository<EconomyYear> repository,
            IItProjectRepository projectRepository)
            : base(repository)
        {
            _projectRepository = projectRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<EconomyYear, ItProject>(x => _projectRepository.GetById(x.ItProjectId), base.GetCrudAuthorization());
        }

        protected override IQueryable<EconomyYear> GetAllQuery()
        {
            var query = Repository.AsQueryable();
            if (AuthorizationContext.GetCrossOrganizationReadAccess() == CrossOrganizationDataReadAccessLevel.All)
            {
                return query;
            }

            var organizationIds = UserContext.OrganizationIds.ToList();

            return query.Where(x => organizationIds.Contains(x.ItProject.OrganizationId));
        }
    }
}
