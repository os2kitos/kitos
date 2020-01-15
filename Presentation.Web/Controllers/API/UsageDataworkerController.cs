using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Repositories.SystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
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
            return new ChildEntityCrudAuthorization<ItSystemUsageDataWorkerRelation, ItSystemUsage>(x => _systemUsageRepository.GetSystemUsage(x.ItSystemUsageId), base.GetCrudAuthorization());
        }
    }
}