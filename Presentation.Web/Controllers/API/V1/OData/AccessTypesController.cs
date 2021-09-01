using Core.DomainServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Repositories.System;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [PublicApi]
    public class AccessTypesController : BaseEntityController<AccessType>
    {
        private readonly IItSystemRepository _systemRepository;

        public AccessTypesController(IGenericRepository<AccessType> repository, IItSystemRepository systemRepository)
            : base(repository)
        {
            _systemRepository = systemRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<AccessType, ItSystem>(accessType => _systemRepository.GetSystem(accessType.ItSystemId), base.GetCrudAuthorization());
        }
    }
}