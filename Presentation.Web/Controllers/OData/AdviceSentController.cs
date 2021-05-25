using System;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainServices;
using Core.DomainServices.Advice;
using Microsoft.AspNet.OData;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.OData
{
    [InternalApi]
    public class AdviceSentController : BaseEntityController<AdviceSent>
    {
        private readonly IGenericRepository<Advice> _adviceRepository;
        private readonly IAdviceRootResolution _adviceRootResolution;

        public AdviceSentController(
            IGenericRepository<AdviceSent> repository,
            IGenericRepository<Advice> adviceRepository,
            IAdviceRootResolution adviceRootResolution) :
            base(repository)
        {
            _adviceRepository = adviceRepository;
            _adviceRootResolution = adviceRootResolution;
        }

        public override IHttpActionResult Get()
        {
            //TODO: Must not fallback to standard access control since that is not enough without global read access
            return base.Get();
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<AdviceSent, IEntityWithAdvices>(ResolveRoot, base.GetCrudAuthorization());
        }

        private IEntityWithAdvices ResolveRoot(Advice advice)
        {
            return _adviceRootResolution.Resolve(advice).GetValueOrDefault();
        }

        private IEntityWithAdvices ResolveRoot(AdviceSent relation)
        {
            if (relation?.AdviceId.HasValue == true)
            {
                var advice = _adviceRepository.GetByKey(relation.AdviceId.Value);
                if (advice != null)
                {
                    return ResolveRoot(advice);
                }
            }

            return null;
        }

        [NonAction]
        public override IHttpActionResult Post(int organizationId, AdviceSent entity) => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Patch(int key, Delta<AdviceSent> delta) => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Delete(int key) => throw new NotSupportedException();
    }
}