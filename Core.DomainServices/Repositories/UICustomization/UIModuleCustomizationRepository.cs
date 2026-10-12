using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.Events;
using Core.DomainModel.UIConfiguration;
namespace Core.DomainServices.Repositories.UICustomization
{
    public class UIModuleCustomizationRepository : IUIModuleCustomizationRepository
    {
        private readonly IGenericRepository<UIModuleCustomization> _repository;
        private readonly IGenericRepository<CustomizedUINode> _nodesRepository;
        private readonly IDomainEvents _domainEvents;

        public UIModuleCustomizationRepository(
            IGenericRepository<UIModuleCustomization> repository,
            IGenericRepository<CustomizedUINode> nodesRepository,
            IDomainEvents domainEvents)
        {
            _repository = repository;
            _nodesRepository = nodesRepository;
            _domainEvents = domainEvents;
        }

        public Maybe<UIModuleCustomization> GetByOrganizationAndModule(int organizationId, string module)
        {
            return _repository.GetWithReferencePreload(x => x.Nodes)
                .SingleOrDefault(x => x.OrganizationId == organizationId && x.Module == module);
        }

        public void DeleteNodes(IEnumerable<CustomizedUINode> nodes)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));

            _nodesRepository.RemoveRange(nodes);
        }

        public void Update(UIModuleCustomization uiModuleCustomization)
        {
            // New modules have not been saved when there were no nodes to delete.
            // Marking them as modified would attempt to update a temporary key.
            if (uiModuleCustomization.Id == 0)
                _repository.Insert(uiModuleCustomization);
            else
                _repository.Update(uiModuleCustomization);
            _domainEvents.Raise(new EntityUpdatedEvent<UIModuleCustomization>(uiModuleCustomization));
            _repository.Save();
        }
    }
}
