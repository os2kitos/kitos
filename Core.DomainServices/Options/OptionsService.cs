﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model.Options;


namespace Core.DomainServices.Options
{
    public class OptionsService<TReference, TOption, TLocalOption> : IOptionsService<TReference, TOption>
        where TOption : OptionEntity<TReference>
        where TLocalOption : LocalOptionEntity<TOption>
    {
        private readonly IGenericRepository<TLocalOption> _localOptionRepository;
        private readonly IGenericRepository<TOption> _optionRepository;

        public OptionsService(
            IGenericRepository<TLocalOption> localOptionRepository,
            IGenericRepository<TOption> optionRepository)
        {
            _localOptionRepository = localOptionRepository;
            _optionRepository = optionRepository;
        }

        public IEnumerable<OptionDescriptor<TOption>> GetAvailableOptionsDetails(int organizationId)
        {
            var allOptions = GetAvailableOptionsFromOrganization(organizationId);

            return allOptions
                .descriptors
                .OrderBy(option => option.Option.Name)
                .ToList();
        }

        public IEnumerable<(OptionDescriptor<TOption> option, bool available)> GetAllOptionsDetails(int organizationId)
        {
            var availableDescriptors = GetAvailableOptionsFromOrganization(organizationId).descriptors.ToDictionary(x => x.Option.Id);
            return _optionRepository
                .AsQueryable()
                .ToList()
                .Select(option => availableDescriptors.TryGetValue(option.Id, out var availableOption)
                    ? (availableOption, true)
                    : (new OptionDescriptor<TOption>(option, option.Description), false))
                .ToList();
        }

        public IEnumerable<TOption> GetAvailableOptions(int organizationId)
        {
            return GetAvailableOptionsFromOrganization(organizationId)
                .rawOptions
                .OrderBy(x => x.Name)
                .ToList();
        }

        private (IEnumerable<OptionDescriptor<TOption>> descriptors, IQueryable<TOption> rawOptions) GetAvailableOptionsFromOrganization(int organizationId)
        {
            var localOptions = _localOptionRepository
                .AsQueryable()
                .ByOrganizationId(organizationId);

            var activeOptionIds = localOptions
                .Where(x => x.IsActive)
                .Select(x => x.OptionId)
                .ToList();

            // LINQ doesn't like IsNullOrWhiteSpace("")
            var localDescriptions = localOptions
                .Where(x => !(x.Description == null || x.Description.Trim() == string.Empty))
                .ToDictionary(x => x.OptionId, x => x.Description);

            var allLocallyEnabled = _optionRepository
                .AsQueryable()
                .Where(x => x.IsEnabled) //Local cannot include not-enabled options
                .ByIds(activeOptionIds);

            var allObligatory = _optionRepository
                .AsQueryable()
                .Where(x => x.IsEnabled)
                .ExceptEntitiesWithIds(activeOptionIds)
                .Where(x => x.IsObligatory); //Add enabled global options which are obligatory as well

            var allOptions = allObligatory.Concat(allLocallyEnabled);
            return (allOptions
                .AsEnumerable()
                .Select(x => new OptionDescriptor<TOption>(x,
                    localDescriptions.ContainsKey(x.Id) ? localDescriptions[x.Id] : x.Description)), allOptions);
        }

        public Maybe<TOption> GetAvailableOption(int organizationId, int optionId)
        {
            return GetAvailableOptionsFromOrganization(organizationId).rawOptions.FirstOrDefault(option => option.Id == optionId);
        }

        public Maybe<(TOption option, bool available)> GetOption(int organizationId, int optionId)
        {
            return
                GetAvailableOption(organizationId, optionId)
                    .Match
                    (
                        onValue: option => (option, true),
                        onNone: () => _optionRepository
                            .GetByKey(optionId)
                            .FromNullable()
                            .Select(option => (option, false))
                    );
        }

        public Maybe<(TOption option, bool available)> GetOptionByUuid(int organizationId, Guid optionUuid)
        {
            return _optionRepository
                .AsQueryable()
                .ByUuid(optionUuid)
                .FromNullable()
                .Match
                (
                    foundOption => GetOption(organizationId, foundOption.Id),
                    () => Maybe<(TOption option, bool available)>.None
                );
        }
    }
}
