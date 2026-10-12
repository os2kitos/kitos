using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Contract.ReadModels;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class ItContractSupplierOverviewReadModelsController : BaseOdataController
    {
        private readonly IItContractSupplierOverviewReadModelsService _readModelsService;
        private readonly IEntityIdentityResolver _identityResolver;

        public ItContractSupplierOverviewReadModelsController(
            IItContractSupplierOverviewReadModelsService readModelsService,
            IEntityIdentityResolver identityResolver)
        {
            _readModelsService = readModelsService;
            _identityResolver = identityResolver;
        }

        [EnableQuery(MaxNodeCount = 300)]
        [Route("odata/ItContractSupplierOverviewReadModels")]
        public IActionResult Get(Guid organizationUuid)
        {
            var orgDbId = _identityResolver.ResolveDbId<Organization>(organizationUuid);
            if (orgDbId.IsNone)
            {
                return FromOperationError(new OperationError("Invalid org id", OperationFailure.NotFound));
            }

            return _readModelsService
                .GetByOrganizationId(orgDbId.Value)
                .Match(onSuccess: q => Ok(q.ToList()), onFailure: FromOperationError);
        }
    }
}
