using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainServices.Model.Options;
using System;
using System.Collections.Generic;

namespace Core.DomainServices.Options
{
    public interface IRoleOptionsService<TReference, TOption> where TOption : OptionEntity<TReference>
    {
        /// <summary>
        /// Returns a list of options available to the organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns>An IEnumerable of <see cref="OptionDescriptor{TOption}"/> containing only available options with their details</returns>
        IEnumerable<OptionDescriptor<TOption>> GetAvailableOptionsDetails(int organizationId);
    }
}
