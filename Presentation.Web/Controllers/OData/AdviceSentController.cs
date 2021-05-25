using System;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainServices;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.Project;
using Core.DomainServices.Repositories.SystemUsage;
using Microsoft.AspNet.OData;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.OData
{
    [InternalApi]
    public class AdviceSentController : BaseEntityController<AdviceSent>
    {
        private readonly IGenericRepository<Advice> _adviceRepository;
        private readonly IItSystemUsageRepository _itSystemUsageRepository;
        private readonly IItProjectRepository _itProjectRepository;
        private readonly IItContractRepository _itContractRepository;
        private readonly IDataProcessingRegistrationRepository _dataProcessingRegistrationRepository;

        public AdviceSentController(
            IGenericRepository<AdviceSent> repository,
            IGenericRepository<Advice> adviceRepository,
            IItSystemUsageRepository itSystemUsageRepository,
            IItProjectRepository itProjectRepository,
            IItContractRepository itContractRepository,
            IDataProcessingRegistrationRepository dataProcessingRegistrationRepository) :
            base(repository)
        {
            _adviceRepository = adviceRepository;
            _itSystemUsageRepository = itSystemUsageRepository;
            _itProjectRepository = itProjectRepository;
            _itContractRepository = itContractRepository;
            _dataProcessingRegistrationRepository = dataProcessingRegistrationRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<AdviceSent, IEntityWithAdvices>(ResolveRoot, base.GetCrudAuthorization());
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