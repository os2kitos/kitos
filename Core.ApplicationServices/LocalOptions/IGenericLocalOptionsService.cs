using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.LocalOptions;
using Core.DomainModel;

namespace Core.ApplicationServices.LocalOptions
{
    public interface IGenericLocalOptionsService<TLocalModelType, TDomainModelType, TOptionType>
        where TLocalModelType : LocalOptionEntity<TOptionType>, new()
        where TOptionType : OptionEntity<TDomainModelType>
    {
        Result<IEnumerable<TOptionType>, OperationError> GetByOrganizationUuid(Guid organizationUuid);
        Result<TOptionType, OperationError> GetByOrganizationAndRoleUuid(Guid organizationUuid, int roleId);

        Result<TLocalModelType, OperationError> CreateLocalOption(Guid organizationUuid,
            LocalOptionCreateParameters parameters);

        Result<TLocalModelType, OperationError> PatchLocalOption(Guid organizationUuid, int optionId,
            LocalOptionUpdateParameters parameters);

        Result<TLocalModelType, OperationError> DeleteLocalOption(Guid organizationUuid, int optionId);
    }
}
