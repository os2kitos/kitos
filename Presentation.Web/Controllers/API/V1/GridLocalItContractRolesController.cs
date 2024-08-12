using Core.DomainModel.ItContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Core.Abstractions.Types;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Generic;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    public class GridLocalItContractRolesController : BaseApiController
    {
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly IGenericRepository<LocalItContractRole> _repository;
        private readonly IGenericRepository<ItContractRole> _optionsRepository;
        public GridLocalItContractRolesController(IGenericRepository<LocalItContractRole> repository, IGenericRepository<ItContractRole> optionsRepository, IEntityIdentityResolver entityIdentityResolver)
        {
            _entityIdentityResolver = entityIdentityResolver;
            _repository = repository;
            _optionsRepository = optionsRepository;
        }

        public HttpResponseMessage GetByOrganizationUuid(Guid organizationUuid)
        {
            var organizationIdResult = _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid);
            if (organizationIdResult.IsNone)
                return FromOperationError(new OperationError("Invalid organization uuid", OperationFailure.NotFound));

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
            return Ok(returnList);
        }
    }
}