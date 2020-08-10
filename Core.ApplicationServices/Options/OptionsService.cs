using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Options
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

        public IEnumerable<TOption> GetAvailableOptions(int organizationId)
        {
            var allOptions = GetAvailableOptionsFromOrganization(organizationId);

            return allOptions
                .OrderBy(option => option.Name)
                .ToList();
        }

        private IQueryable<TOption> GetAvailableOptionsFromOrganization(int organizationId)
        {
            var activeOptionIds = _localOptionRepository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .Where(x => x.IsActive)
                .Select(x => x.OptionId)
                .ToList();

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
            return allOptions;
        }

        public Maybe<TOption> GetAvailableOption(int organizationId, int optionId)
        {
            return GetAvailableOptionsFromOrganization(organizationId)
                .FirstOrDefault(option => option.Id == optionId);
        }
    }
}
