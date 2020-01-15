using Core.DomainModel.ItProject;
using Core.DomainServices;
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
            return new ChildEntityCrudAuthorization<Assignment, ItProject>(x => _projectRepository.GetById(x.AssociatedItProjectId.GetValueOrDefault(-1)), base.GetCrudAuthorization());
        }
    }
}
