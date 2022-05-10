using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.DomainModel.UIConfiguration;

namespace Core.DomainServices.UIConfiguration
{
    public interface IUIVisibilityConfigurationRepository
    {
        void UpdateRange(IEnumerable<UIVisibilityConfiguration> entities);
        IQueryable<UIVisibilityConfiguration> GetModuleConfigurationForOrganization(int organizationId, string module);
    }
}
