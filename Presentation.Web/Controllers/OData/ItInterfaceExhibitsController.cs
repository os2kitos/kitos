using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Extensions;
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
            IAuthenticationService authService,
            IAuthorizationContext authorizationContext,
            IGenericRepository<ItInterface> interfaceRepository)
            : base(repository, authService, authorizationContext)
        {
            _interfaceRepository = interfaceRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ItInterfaceExhibit>(x => _interfaceRepository.AsQueryable().ById(x.Id), base.GetCrudAuthorization());
        }
    }
}
