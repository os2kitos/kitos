using System;
using Core.DomainModel;
using Core.DomainServices;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainServices.Extensions;

namespace Core.ApplicationServices.LocalOptions.Base
{
    public class GenericLocalOptionsService<TLocalModelType, TDomainModelType, TOptionType> : IGenericLocalOptionsService<TLocalModelType, TDomainModelType, TOptionType>
    where TLocalModelType : LocalOptionEntity<TOptionType>, new ()
    where TOptionType : OptionEntity<TDomainModelType>
    {
        private readonly IGenericRepository<TOptionType> _optionsRepository;
        private readonly IGenericRepository<TLocalModelType> _localOptionRepository;

        public GenericLocalOptionsService(IGenericRepository<TOptionType> optionsRepository, IGenericRepository<TLocalModelType> localOptionRepository)
        {
            _optionsRepository = optionsRepository;
            _localOptionRepository = localOptionRepository;
        }


        public Result<IEnumerable<TOptionType>, OperationError> GetByOrganizationUuid(Guid organizationUuid)
        {
            var localOptionsResult =
                _localOptionRepository
                    .AsQueryable()
                    .ByOrganizationUuid(organizationUuid)
                    .ToDictionary(x => x.OptionId);

            var globalOptionsResult = _optionsRepository
                .AsQueryable()
                .Where(x => x.IsEnabled)
                .ToList();

            var returnList = new List<TOptionType>();

            foreach (var item in globalOptionsResult)
            {
                var itemToAdd = item;
                itemToAdd.IsLocallyAvailable = false;


                if (localOptionsResult.TryGetValue(item.Id, out var localOption))
                {
                    itemToAdd.IsLocallyAvailable = localOption.IsActive;
                    if (!string.IsNullOrEmpty(localOption.Description))
                    {
                        itemToAdd.Description = localOption.Description;
                    }
                }

                returnList.Add(itemToAdd);
            }
            return returnList;
        }
    }
}
