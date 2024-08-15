using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Core.DomainServices;
using Core.DomainServices.Extensions;

namespace Core.ApplicationServices.Contract.ReadModels
{
    public class GridLocalItContractRolesService : IGridLocalItContractRolesService
    {
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly IGenericRepository<LocalItContractRole> _repository;
        private readonly IGenericRepository<ItContractRole> _optionsRepository;

        public GridLocalItContractRolesService(IEntityIdentityResolver entityIdentityResolver, IGenericRepository<LocalItContractRole> repository, IGenericRepository<ItContractRole> optionsRepository)
        {
            _entityIdentityResolver = entityIdentityResolver;
            _repository = repository;
            _optionsRepository = optionsRepository;
        }

        public Result<IEnumerable<(string Name, Guid Uuid, int Id)>, OperationError> GetOverviewRoles(Guid organizationUuid)
        {
            var organizationIdResult = _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid);
            if (organizationIdResult.IsNone)
                return new OperationError("Invalid organization uuid", OperationFailure.NotFound);

            var organizationId = organizationIdResult.Value;
            var localOptionsResult =
                _repository
                    .AsQueryable()
                    .ByOrganizationId(organizationId)
                    .ToDictionary(x => x.OptionId);

            var globalOptionsResult = _optionsRepository
                .AsQueryable()
                .Where(x => x.IsEnabled)
                .ToList();

            var returnList = new List<ItContractRole>();

            foreach (var item in globalOptionsResult)
            {
                item.IsLocallyAvailable = false;


                if (localOptionsResult.TryGetValue(item.Id, out var localOption))
                {
                    item.IsLocallyAvailable = localOption.IsActive;
                    if (!string.IsNullOrEmpty(localOption.Description))
                    {
                        item.Description = localOption.Description;
                    }
                }

                returnList.Add(item);
            }
            return returnList.Select(x => (x.Name,x.Uuid, x.Id)).ToList();
        }
    }
}
