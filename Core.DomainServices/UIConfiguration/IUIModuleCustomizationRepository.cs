using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.DomainModel.UIConfiguration;

namespace Core.DomainServices.UIConfiguration
{
    public interface IUIModuleCustomizationRepository
    {
        void UpdateRange(IEnumerable<UIModuleCustomization> entities);
        IQueryable<UIModuleCustomization> GetModuleConfigurationForOrganization(int organizationId, string module);
    }
}
