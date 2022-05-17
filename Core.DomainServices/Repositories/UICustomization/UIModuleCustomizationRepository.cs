using System.Linq;
using Core.DomainModel.Events;
using Core.DomainModel.UIConfiguration;

namespace Core.DomainServices.Repositories.UICustomization
{
    public class UIModuleCustomizationRepository : IUIModuleCustomizationRepository
    {
        private readonly IGenericRepository<UIModuleCustomization> _repository;
        private readonly IDomainEvents _domainEvents;

        public UIModuleCustomizationRepository(IGenericRepository<UIModuleCustomization> repository, IDomainEvents domainEvents)
        {
            _repository = repository;
            _domainEvents = domainEvents;
        }

        public void Update(DomainModel.Organization.Organization organization, UIModuleCustomization uiModuleCustomization)
        {
            _domainEvents.Raise(new EntityUpdatedEvent<DomainModel.Organization.Organization>(organization));
            _repository.Save();
        }
    }
}
