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

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<Stakeholder>(x => _projectRepository.GetById(x.ItProjectId), base.GetCrudAuthorization());
        }
    }
}
