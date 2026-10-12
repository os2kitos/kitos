using Core.DomainModel;
using Core.DomainServices.Model.Options;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Core.DomainModel.LocalOptions;
using Core.DomainServices.Extensions;

namespace Core.DomainServices.Options
{
    public class RoleOptionsService<TReference, TOption, TLocalOption> : IRoleOptionsService<TReference, TOption>
        where TOption : OptionEntity<TReference>
        where TLocalOption : LocalRoleOptionEntity<TOption>
    {
        private readonly IGenericRepository<TLocalOption> _localOptionRepository;
        private readonly IGenericRepository<TOption> _optionRepository;

        public RoleOptionsService(
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
                .OrderBy(option => option.Option.Name)
                .ToList();
        }

        private IEnumerable<OptionDescriptor<TOption>> GetAvailableOptionsFromOrganization(int organizationId)
        {
            var localOptions = _localOptionRepository
                .AsQueryable()
                .ByOrganizationId(organizationId);

            var activeOptionIds = localOptions
                .Where(x => x.IsActive)
                .Select(x => x.OptionId)
                .ToList();

            // LINQ doesn't like IsNullOrWhiteSpace("")
            var localOptionsDictionary = localOptions
                .Where(x => !(x.Description == null || x.Description.Trim() == string.Empty) || x.IsExternallyUsed)
                .ToDictionary(x => x.OptionId, x =>(description: x.Description, isExternallyUsed: x.IsExternallyUsed, externallyUsedDescription: x.ExternallyUsedDescription));

            var allLocallyEnabled = GetEnabledOptions() //Local cannot include not-enabled options
                .ByIds(activeOptionIds);

            var allObligatory = GetEnabledOptions()
                .ExceptEntitiesWithIds(activeOptionIds)
                .Where(x => x.IsObligatory); //Add enabled global options which are obligatory as well

            var allOptions = allObligatory.Concat(allLocallyEnabled);
            return allOptions
                .AsEnumerable()
                .Select(x =>
                {
                    if (!localOptionsDictionary.ContainsKey(x.Id))
                        return x;

                    var localOption = localOptionsDictionary[x.Id];
                    x.UpdateLocalExternalUsedValues(localOption.isExternallyUsed, localOption.externallyUsedDescription);
                    return x;
                })
                .Select(x => new OptionDescriptor<TOption>(x,
                    localOptionsDictionary.ContainsKey(x.Id) ? localOptionsDictionary[x.Id].description : x.Description)).ToList();
        }

        private IQueryable<TOption> GetEnabledOptions()
        {
            return _optionRepository
                .AsQueryable()
                .Where(x => x.IsEnabled);
        }
    }
}
