using System.Collections.Generic;
using Core.DomainModel;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Options
{
    public interface IOptionsService<TReference, TOption> where TOption : OptionEntity<TReference>
    {
        /// <summary>
        /// Returns a list of options available to the organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        IEnumerable<TOption> GetAvailableOptions(int organizationId);
        /// <summary>
        /// Returns the option if it is available in the organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="optionId"></param>
        /// <returns></returns>
        Maybe<TOption> GetAvailableOption(int organizationId, int optionId);
        /// <summary>
        /// Returns the option if it exists and a boolean indicating whether it is available
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="optionId"></param>
        /// <returns></returns>
        Maybe<(TOption option, bool available)> GetOption(int organizationId, int optionId);
    }
}
