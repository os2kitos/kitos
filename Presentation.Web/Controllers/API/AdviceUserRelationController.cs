using Core.DomainModel.Advice;
using Core.DomainServices;
using Presentation.Web.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.Project;
using Core.DomainServices.Repositories.SystemUsage;
using Infrastructure.Services.DomainEvents;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class AdviceUserRelationController : GenericApiController<AdviceUserRelation, AdviceUserRelationDTO>
    {
        private readonly IGenericRepository<AdviceUserRelation> _repository;
        private readonly IGenericRepository<Advice> _adviceRepository;
        private readonly IItSystemUsageRepository _itSystemUsageRepository;
        private readonly IItProjectRepository _itProjectRepository;
        private readonly IItContractRepository _itContractRepository;
        private readonly IDataProcessingRegistrationRepository _dataProcessingRegistrationRepository;

        public AdviceUserRelationController(
            IGenericRepository<AdviceUserRelation> repository,
            IGenericRepository<Advice> adviceRepository,
            IItSystemUsageRepository itSystemUsageRepository,
            IItProjectRepository itProjectRepository,
            IItContractRepository itContractRepository,
            IDataProcessingRegistrationRepository dataProcessingRegistrationRepository)
            : base(repository)
        {
            _repository = repository;
            _adviceRepository = adviceRepository;
            _itSystemUsageRepository = itSystemUsageRepository;
            _itProjectRepository = itProjectRepository;
            _itContractRepository = itContractRepository;
            _dataProcessingRegistrationRepository = dataProcessingRegistrationRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<AdviceUserRelation, IEntityWithAdvices>(ResolveRoot, base.GetCrudAuthorization());
        }

        //TODO: To helpers
        private IEntityWithAdvices ResolveRoot(Advice advice)
        {
            if (advice.Type != null && advice.RelationId != null)
            {
                var adviceRelationId = advice.RelationId.Value;

                switch (advice.Type)
                {
                    case ObjectType.itContract:
                        return _itContractRepository.GetById(adviceRelationId);
                    case ObjectType.itSystemUsage:
                        return _itSystemUsageRepository.GetSystemUsage(adviceRelationId);
                    case ObjectType.itProject:
                        return _itProjectRepository.GetById(adviceRelationId);
                    case ObjectType.dataProcessingRegistration:
                        return _dataProcessingRegistrationRepository.GetById(adviceRelationId).GetValueOrDefault();
                    case ObjectType.itInterface: //Intended fallthrough
                    default:
                        throw new NotSupportedException("Unsupported object type:" + advice.Type);
                }
            }

            return null;
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

        //TODO: To helpers
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
                case ItProject root:
                    DomainEvents.Raise(new EntityUpdatedEvent<ItProject>(root));
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
                foreach (var d in _repository.AsQueryable().Where(d => d.AdviceId == adviceId).ToList())
                {
                    if (AllowDelete(d))
                    {
                        _repository.Delete(d);
                        _repository.Save();
                    }
                    else
                    {
                        return Forbidden();
                    }
                }
                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}