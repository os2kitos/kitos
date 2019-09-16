using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class UsageDataworkerController : GenericApiController<ItSystemUsageDataWorkerRelation, ItSystemUsageDataWorkerRelationDTO>
    {
        private readonly IItSystemUsageService _systemUsageService;

        public UsageDataworkerController(
            IGenericRepository<ItSystemUsageDataWorkerRelation> repository,
            IItSystemUsageService systemUsageService, IAuthorizationContext authorizationContext)
            : base(repository, authorizationContext)
        {
            _systemUsageService = systemUsageService;
        }

        protected override bool AllowCreate<T>(IEntity entity)
        {
            if (entity is ItSystemUsageDataWorkerRelation relation)
            {
                return _systemUsageService.CanAddDataWorkerRelation(relation.ItSystemUsageId, relation.DataWorkerId);
            }
            return false;
        }

        protected override bool AllowModify(IEntity entity)
        {
            return AllowModifyAssociatedSystemUsage(entity);
        }

        protected override bool AllowDelete(IEntity entity)
        {
            return AllowModifyAssociatedSystemUsage(entity);
        }

        private bool AllowModifyAssociatedSystemUsage(IEntity entity)
        {
            if (entity is ItSystemUsageDataWorkerRelation relation)
            {
                return base.AllowModify(relation.ItSystemUsage);
            }

            return false;
        }

        protected override bool AllowRead(IEntity entity)
        {
            if (entity is ItSystemUsageDataWorkerRelation relation)
            {
                return base.AllowRead(relation.ItSystemUsage);
            }

            return false;
        }
    }
}