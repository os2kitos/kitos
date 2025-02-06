using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.GlobalOptions;
using Core.DomainModel;

namespace Core.ApplicationServices.GlobalOptions
{
    public interface IGlobalRegularOptionsService<TOptionType, TReferenceType>
        where TOptionType : OptionEntity<TReferenceType>, new()
    {

        Result<IEnumerable<TOptionType>, OperationError> GetGlobalOptions();
        Result<TOptionType, OperationError> CreateGlobalOption(GlobalRegularOptionCreateParameters createParameters);

        Result<TOptionType, OperationError> PatchGlobalOption(Guid optionUuid, GlobalRegularOptionUpdateParameters updateParameters);
    }
}
