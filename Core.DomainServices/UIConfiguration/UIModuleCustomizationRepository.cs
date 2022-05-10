using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Events;
using Core.DomainModel.UIConfiguration;

namespace Core.DomainServices.UIConfiguration
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

        public void UpdateRange(IEnumerable<UIModuleCustomization> entities)
        {
            //TODO: domainEvents.Raise<EntityUpdated<Organization>>()
            _repository.Save();
        }

        public IQueryable<UIModuleCustomization> GetModuleConfigurationForOrganization(int organizationId, string module)
        {
            return _repository.AsQueryable()
                .Where(x => x.OrganizationId == organizationId && string.Equals(x.Module, module));
        }
    }
}
