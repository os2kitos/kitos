using System;
using Core.DomainModel;
using Core.DomainServices;
using Core.DomainServices.Repositories.SystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.OData.AttachedOptions
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
    }
}