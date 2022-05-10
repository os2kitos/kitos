using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainModel.UIConfiguration;

namespace Core.DomainServices.UIConfiguration
{
    public class UIVisibilityConfigurationRepository : IUIVisibilityConfigurationRepository
    {
        private readonly IGenericRepository<UIVisibilityConfiguration> _repository;
        private readonly IDomainEvents _domainEvents;

        public UIVisibilityConfigurationRepository(IGenericRepository<UIVisibilityConfiguration> repository, IDomainEvents domainEvents)
        {
            _repository = repository;
            _domainEvents = domainEvents;
        }

        public void UpdateRange(IEnumerable<UIVisibilityConfiguration> entities)
        {
            //TODO: domainEvents.Raise<EntityUpdated<Organization>>()
            _repository.Save();
        }

        public IQueryable<UIVisibilityConfiguration> GetModuleConfigurationForOrganization(int organizationId, string module)
        {
            return _repository.AsQueryable()
                .Where(x => x.OrganizationId == organizationId && string.Equals(x.Module, module));
        }
    }
}
