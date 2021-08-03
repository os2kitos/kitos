using Core.DomainModel;
using Core.DomainModel.Constants;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Repositories.Project;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [PublicApi]
    public class ItProjectStatusUpdatesController : BaseEntityController<ItProjectStatusUpdate>
    {
        private readonly IItProjectRepository _projectRepository;

        public ItProjectStatusUpdatesController(IGenericRepository<ItProjectStatusUpdate> repository, IItProjectRepository projectRepository)
        : base(repository)
        {
            _projectRepository = projectRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ItProjectStatusUpdate, ItProject>(ps => _projectRepository.GetById(ps.AssociatedItProjectId.GetValueOrDefault(EntityConstants.InvalidId)), base.GetCrudAuthorization());
        }
    }
}
