using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.Options
{
    public interface IOptionsService<TReference, TOption> where TOption : OptionEntity<TReference>
    {
        IEnumerable<TOption> GetAvailableOptions(int organizationId);
        Maybe<TOption> GetAvailableOption(int organizationId, int optionId);
    }
}
