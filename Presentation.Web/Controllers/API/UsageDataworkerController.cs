using System;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
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
            return GeAuthorizationFromRoot(entity, base.AllowModify);
        }

        protected override bool AllowDelete(IEntity entity)
        {
            //Check if modification, not deletion, of parent usage (the root aggregate) is allowed 
            return GeAuthorizationFromRoot(entity, base.AllowModify);
        }

        protected override bool AllowRead(IEntity entity)
        {
            return GeAuthorizationFromRoot(entity, base.AllowRead);
        }

        private static bool GeAuthorizationFromRoot(IEntity entity, Predicate<ItSystemUsage> condition)
        {
            if (entity is ItSystemUsageDataWorkerRelation relation)
            {
                return condition.Invoke(relation.ItSystemUsage);
            }

            return false;
        }
    }
}