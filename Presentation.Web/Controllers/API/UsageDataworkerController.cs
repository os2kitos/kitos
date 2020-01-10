using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Repositories.SystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class UsageDataworkerController : GenericApiController<ItSystemUsageDataWorkerRelation, ItSystemUsageDataWorkerRelationDTO>
    {
        private readonly IItSystemUsageRepository _systemUsageRepository;

        public UsageDataworkerController(IGenericRepository<ItSystemUsageDataWorkerRelation> repository, IItSystemUsageRepository systemUsageRepository)
            : base(repository)
        {
            _systemUsageRepository = systemUsageRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ItSystemUsageDataWorkerRelation>(x => _systemUsageRepository.GetSystemUsage(x.ItSystemUsageId), base.GetCrudAuthorization());
        }
    }
}