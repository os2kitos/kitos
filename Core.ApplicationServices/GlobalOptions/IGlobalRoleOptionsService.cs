
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.GlobalOptions;
using Core.DomainModel;
using System.Collections.Generic;
using System;

namespace Core.ApplicationServices.GlobalOptions
{
    public interface IGlobalRoleOptionsService<TOptionType, TReferenceType>
        where TOptionType : OptionEntity<TReferenceType>, IRoleEntity, new()
    {
        Result<IEnumerable<TOptionType>, OperationError> GetGlobalOptions();
        Result<TOptionType, OperationError> CreateGlobalOption(GlobalRoleOptionCreateParameters createParameters);

        Result<TOptionType, OperationError> PatchGlobalOption(Guid optionUuid, GlobalRoleOptionUpdateParameters updateParameters);
    }
}
