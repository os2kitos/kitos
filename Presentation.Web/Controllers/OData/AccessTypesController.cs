using Core.DomainServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Repositories.System;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
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
            //TODO: Look for places where repo is not used but just the reference.. That will fail on CanCreate .. 
            return new ChildEntityCrudAuthorization<AccessType>(accessType => _systemRepository.GetSystem(accessType.ItSystemId), base.GetCrudAuthorization());
        }
    }
}