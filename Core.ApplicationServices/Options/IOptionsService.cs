using System.Collections.Generic;
using Core.DomainModel;

namespace Core.ApplicationServices.Options
{
    public interface IOptionsService<TReference, out TOption> where TOption : OptionEntity<TReference>
    {
        IEnumerable<TOption> GetAvailableOptions(int organizationId);
    }
}
