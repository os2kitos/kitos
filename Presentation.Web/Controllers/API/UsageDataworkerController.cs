using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Repositories.SystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [ControllerEvaluationCompleted]
    public class UsageDataworkerController : GenericApiController<ItSystemUsageDataWorkerRelation, ItSystemUsageDataWorkerRelationDTO>
    {
        private readonly IItSystemUsageRepository _systemUsageRepository;

        public UsageDataworkerController(
            IGenericRepository<ItSystemUsageDataWorkerRelation> repository,
            IItSystemUsageRepository systemUsageRepository,
            IAuthorizationContext authorizationContext)
            : base(repository, authorizationContext)
        {
            _systemUsageRepository = systemUsageRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ItSystemUsageDataWorkerRelation>(x => _systemUsageRepository.GetSystemUsage(x.ItSystemUsageId), base.GetCrudAuthorization());
        }
    }
}