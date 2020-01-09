using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Repositories.Project;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class EconomyYearController : GenericContextAwareApiController<EconomyYear, EconomyYearDTO>
    {
        private readonly IItProjectRepository _projectRepository;

        public EconomyYearController(
            IGenericRepository<EconomyYear> repository,
            IAuthorizationContext authorizationContext,
            IItProjectRepository projectRepository) 
            : base(repository, authorizationContext)
        {
            _projectRepository = projectRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<EconomyYear>(x => _projectRepository.GetById(x.ItProjectId), base.GetCrudAuthorization());
        }
    }
}
