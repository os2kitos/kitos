using System;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Repositories.SystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API.V1.OData.AttachedOptions
{
    [PublicApi]
    public class AttachedOptionsController : BaseEntityController<AttachedOption>
    {
        private readonly IItSystemUsageRepository _usageRepository;

        public AttachedOptionsController(IGenericRepository<AttachedOption> repository, IItSystemUsageRepository usageRepository)
               : base(repository)
        {
            _usageRepository = usageRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<AttachedOption, IHasAttachedOptions>(option => GetOwner(option.ObjectType, option.ObjectId), base.GetCrudAuthorization());
        }

        private IHasAttachedOptions GetOwner(EntityType optionObjectType, int optionObjectId)
        {
            switch (optionObjectType)
            {
                case EntityType.ITSYSTEMUSAGE:
                    return _usageRepository.GetSystemUsage(optionObjectId);
                default:
                    throw new ArgumentOutOfRangeException(nameof(optionObjectType), optionObjectType, null);
            }
        }

        protected override void RaiseCreatedDomainEvent(AttachedOption entity)
        {
            RaiseRootUpdated(entity);
        }

        protected override void RaiseDeletedDomainEvent(AttachedOption entity)
        {
            RaiseRootUpdated(entity);
        }

        protected override void RaiseUpdatedDomainEvent(AttachedOption entity)
        {
            RaiseRootUpdated(entity);
        }

        private void RaiseRootUpdated(AttachedOption entity)
        {
            var itSystemUsage = _usageRepository.GetSystemUsage(entity.ObjectId);
            DomainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(itSystemUsage));
        }
    }
}