using System;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Repositories.System;
using Presentation.Web.Models;
using Presentation.Web.Infrastructure.Attributes;

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

        protected override bool AllowCreate<T>(IEntity entity)
        {
            if (entity is ItSystemDataWorkerRelation relation)
            {
                var itSystem = _systemRepository.GetSystem(relation.ItSystemId);
                return itSystem != null && base.AllowModify(itSystem);
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

        private static bool GeAuthorizationFromRoot(IEntity entity, Predicate<ItSystem> condition)
        {
            if (entity is ItSystemDataWorkerRelation relation)
            {
                return condition.Invoke(relation.ItSystem);
            }

            return false;
        }
    }
}