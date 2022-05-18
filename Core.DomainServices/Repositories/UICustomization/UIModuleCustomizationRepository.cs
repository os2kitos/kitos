using System;
using System.Collections.Generic;
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

        public void DeleteNodes(IEnumerable<CustomizedUINode> nodes)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));

            _nodesRepository.RemoveRange(nodes);
            _repository.Save();
        }

        public void Update(UIModuleCustomization uiModuleCustomization)
        {
            _repository.Update(uiModuleCustomization);
            _domainEvents.Raise(new EntityUpdatedEvent<UIModuleCustomization>(uiModuleCustomization));
            _repository.Save();
        }
    }
}
