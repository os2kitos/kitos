using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    public class ItInterfaceExhibitsController : BaseEntityController<ItInterfaceExhibit>
    {
        private readonly IGenericRepository<ItInterface> _interfaceRepository;

        public ItInterfaceExhibitsController(
            IGenericRepository<ItInterfaceExhibit> repository,
            IGenericRepository<ItInterface> interfaceRepository)
            : base(repository)
        {
            _interfaceRepository = interfaceRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ItInterfaceExhibit, ItInterface>(x => _interfaceRepository.GetByKey(x.Id), base.GetCrudAuthorization());
        }
    }
}
