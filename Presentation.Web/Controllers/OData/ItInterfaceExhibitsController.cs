using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Repositories.System;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    public class ItInterfaceExhibitsController : BaseEntityController<ItInterfaceExhibit>
    {
        private readonly IItSystemRepository _systemRepository;

        public ItInterfaceExhibitsController(
            IGenericRepository<ItInterfaceExhibit> repository, 
            IAuthenticationService authService,
            IAuthorizationContext authorizationContext,
            IItSystemRepository systemRepository)
            : base(repository, authService, authorizationContext)
        {
            _systemRepository = systemRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ItSystemDataWorkerRelation>(x => _systemRepository.GetSystem(x.ItSystemId), base.GetCrudAuthorization());
        }
    }
}
