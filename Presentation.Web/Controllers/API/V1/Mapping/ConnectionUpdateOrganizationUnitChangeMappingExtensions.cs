using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;

namespace Presentation.Web.Controllers.API.V1.Mapping
{
    public static class ConnectionUpdateOrganizationUnitChangeMappingExtensions
    {

        private static readonly IReadOnlyDictionary<ConnectionUpdateOrganizationUnitChangeType, ConnectionUpdateOrganizationUnitChangeCategory> ApiToDataMap;
        private static readonly IReadOnlyDictionary<ConnectionUpdateOrganizationUnitChangeCategory, ConnectionUpdateOrganizationUnitChangeType> DataToApiMap;
        
        static ConnectionUpdateOrganizationUnitChangeMappingExtensions()
        {
            ApiToDataMap = new Dictionary<ConnectionUpdateOrganizationUnitChangeType, ConnectionUpdateOrganizationUnitChangeCategory>
            {
                { ConnectionUpdateOrganizationUnitChangeType.Added, ConnectionUpdateOrganizationUnitChangeCategory.Added},
                { ConnectionUpdateOrganizationUnitChangeType.Renamed, ConnectionUpdateOrganizationUnitChangeCategory.Renamed},
                { ConnectionUpdateOrganizationUnitChangeType.Moved, ConnectionUpdateOrganizationUnitChangeCategory.Moved},
                { ConnectionUpdateOrganizationUnitChangeType.Deleted, ConnectionUpdateOrganizationUnitChangeCategory.Deleted},
                { ConnectionUpdateOrganizationUnitChangeType.Converted, ConnectionUpdateOrganizationUnitChangeCategory.Converted},
            }.AsReadOnly();

            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static ConnectionUpdateOrganizationUnitChangeCategory ToConnectionUpdateOrganizationUnitChangeCategory(this ConnectionUpdateOrganizationUnitChangeType value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }
    }
}