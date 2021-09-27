using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.DomainModel.Tracking;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.Deltas.Mapping
{
    public static class TrackedEntityTypeMappingExtensions
    {
        private static readonly IReadOnlyDictionary<TrackedEntityTypeChoice, TrackedEntityType> ApiToDataMap;
        private static readonly IReadOnlyDictionary<TrackedEntityType, TrackedEntityTypeChoice> DataToApiMap;

        static TrackedEntityTypeMappingExtensions()
        {
            ApiToDataMap = new Dictionary<TrackedEntityTypeChoice, TrackedEntityType>
            {
                { TrackedEntityTypeChoice.DataProcessingRegistration,TrackedEntityType.DataProcessingRegistration },
                { TrackedEntityTypeChoice.ItContract,TrackedEntityType.ItContract },
                { TrackedEntityTypeChoice.ItProject,TrackedEntityType.ItProject },
                { TrackedEntityTypeChoice.ItSystemUsage,TrackedEntityType.ItSystemUsage },
                { TrackedEntityTypeChoice.ItSystem,TrackedEntityType.ItSystem },
                { TrackedEntityTypeChoice.ItInterface,TrackedEntityType.ItInterface }
            }.AsReadOnly();

            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static TrackedEntityType ToDomainType(this TrackedEntityTypeChoice value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static TrackedEntityTypeChoice ToApiType(this TrackedEntityType value)
        {
            return DataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped domain value:{value:G}", nameof(value));
        }
    }
}