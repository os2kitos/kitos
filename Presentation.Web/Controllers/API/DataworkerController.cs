using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Repositories.System;
using Presentation.Web.Models;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class DataworkerController : GenericApiController<ItSystemDataWorkerRelation, ItSystemDataWorkerRelationDTO>
    {
        private readonly IItSystemRepository _systemRepository;

        public DataworkerController(
            IGenericRepository<ItSystemDataWorkerRelation> repository,
            IAuthorizationContext authorization,
            IItSystemRepository systemRepository)
            : base(repository, authorization)
        {
            _systemRepository = systemRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ItSystemDataWorkerRelation>(x => _systemRepository.GetSystem(x.ItSystemId), base.GetCrudAuthorization());
        }
    }
}