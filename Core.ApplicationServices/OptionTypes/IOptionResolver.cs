using System;
using Core.Abstractions.Types;
using Core.DomainModel;

namespace Core.ApplicationServices.OptionTypes
{
    /// <summary>
    /// Resolves status of option types in KITOS
    /// </summary>
    public interface IOptionResolver
    {
        Result<(TOption option, bool available), OperationError> GetOptionType<TReference, TOption>(Guid organizationUuid, Guid optionUuid) where TOption : OptionEntity<TReference>;
    }
}
