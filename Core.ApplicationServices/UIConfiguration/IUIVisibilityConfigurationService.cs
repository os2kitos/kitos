using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.DomainModel.UIConfiguration;

namespace Core.ApplicationServices.UIConfiguration
{
    public interface IUIVisibilityConfigurationService
    {
        Result<List<UIVisibilityConfiguration>, OperationError> GetModuleConfigurationForOrganization(int organizationId, string module);
        Result<List<UIVisibilityConfiguration>, OperationError> Put(int organizationId, string module, List<UIVisibilityConfiguration> configurations);
    }
}
