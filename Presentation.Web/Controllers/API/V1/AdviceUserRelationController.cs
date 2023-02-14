using Core.DomainModel.Advice;
using Core.DomainServices;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Advice;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models.API.V1;
using Core.ApplicationServices.Notification;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    public class AdviceUserRelationController : GenericApiController<AdviceUserRelation, AdviceUserRelationDTO>
    {
        private readonly IGenericRepository<Advice> _adviceRepository;
        private readonly IAdviceRootResolution _adviceRootResolution;
        private readonly IRegistrationNotificationService _registrationNotificationService;

        public AdviceUserRelationController(
            IGenericRepository<AdviceUserRelation> repository,
            IGenericRepository<Advice> adviceRepository,
            IAdviceRootResolution adviceRootResolution,
            IRegistrationNotificationService registrationNotificationService)
            : base(repository)
        {
            _adviceRepository = adviceRepository;
            _adviceRootResolution = adviceRootResolution;
            _registrationNotificationService = registrationNotificationService;
        }

        [NonAction]
        public override HttpResponseMessage GetAll(PagingModel<AdviceUserRelation> paging) => throw new NotSupportedException();

        [NonAction]
        public override HttpResponseMessage GetSingle(int id) => throw new NotSupportedException();

        [NonAction]
        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj) => throw new NotSupportedException();

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<AdviceUserRelation, IEntityWithAdvices>(ResolveRoot, base.GetCrudAuthorization());
        }

        private IEntityWithAdvices ResolveRoot(Advice advice)
        {
            return _adviceRootResolution.Resolve(advice).GetValueOrDefault();
        }

        private IEntityWithAdvices ResolveRoot(AdviceUserRelation relation)
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

        protected override void RaiseDeleted(AdviceUserRelation entity)
        {
            RaiseAsRootModification(entity);
        }

        protected override void RaiseNewObjectCreated(AdviceUserRelation savedItem)
        {
            RaiseAsRootModification(savedItem);
        }

        protected override void RaiseUpdated(AdviceUserRelation item)
        {
            RaiseAsRootModification(item);
        }

        private void RaiseAsRootModification(AdviceUserRelation entity)
        {
            switch (ResolveRoot(entity))
            {
                case ItContract root:
                    DomainEvents.Raise(new EntityUpdatedEvent<ItContract>(root));
                    break;
                case ItSystemUsage root:
                    DomainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(root));
                    break;
                case DataProcessingRegistration root:
                    DomainEvents.Raise(new EntityUpdatedEvent<DataProcessingRegistration>(root));
                    break;
            }
        }

        /// <summary>
        /// Sletter adviser med det specificerede id fra en genereisk advis
        /// </summary>
        /// <param name="adviceId"></param>
        /// <returns></returns>
        [HttpDelete]
        public virtual HttpResponseMessage DeleteByAdviceId(int adviceId)
        {
            try
            {
                return _registrationNotificationService.DeleteUserRelationByAdviceId(adviceId)
                    .Match(FromOperationError, Ok);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}