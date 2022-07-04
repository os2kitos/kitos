using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainServices.Model.Options;


namespace Core.DomainServices.Options
{
    public interface IOptionsService<TReference, TOption> where TOption : OptionEntity<TReference>
    {
        /// <summary>
        /// Returns a list of options available to the organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns>An IEnumerable of <see cref="OptionDescriptor{TOption}"/> containing only available options with their details</returns>
        IEnumerable<OptionDescriptor<TOption>> GetAvailableOptionsDetails(int organizationId);
        /// <summary>
        /// Returns a list of options and a boolean indicating whether it is available
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns>An IEnumerable of <see cref="OptionDescriptor{TOption}"/> containing both available/unavailable options with their details</returns>
        IEnumerable<(OptionDescriptor<TOption> option, bool available)> GetAllOptionsDetails(int organizationId);
        /// <summary>
        /// Returns a list of options available to the organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns>An IEnumerable of <see cref="OptionDescriptor{TOption}"/> containing only available options without details</returns>
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

        /// <summary>
        /// Returns the option if it exists and a boolean indicating whether it is available
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="optionUuid"></param>
        /// <returns></returns>
        Maybe<(TOption option, bool available)> GetOptionByUuid(int organizationId, Guid optionUuid);
    }
}
