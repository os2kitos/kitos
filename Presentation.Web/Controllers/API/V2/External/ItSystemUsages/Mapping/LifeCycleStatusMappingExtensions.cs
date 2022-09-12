using Core.Abstractions.Extensions;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class LifeCycleStatusMappingExtensions
    {
        private static readonly IReadOnlyDictionary<LifeCycleStatusChoice, LifeCycleStatusType> ApiToDataMap;
        private static readonly IReadOnlyDictionary<LifeCycleStatusType, LifeCycleStatusChoice> DataToApiMap;

        static LifeCycleStatusMappingExtensions()
        {
            ApiToDataMap = new Dictionary<LifeCycleStatusChoice, LifeCycleStatusType>
            {
                { LifeCycleStatusChoice.Undecided, LifeCycleStatusType.Undecided },
                { LifeCycleStatusChoice.NotInUse, LifeCycleStatusType.NotInUse },
                { LifeCycleStatusChoice.PhasingIn, LifeCycleStatusType.PhasingIn},
                { LifeCycleStatusChoice.Operational, LifeCycleStatusType.Operational},
                { LifeCycleStatusChoice.PhasingOut, LifeCycleStatusType.PhasingOut}
            }.AsReadOnly();

            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static LifeCycleStatusType ToLifeCycleStatusType(this LifeCycleStatusChoice value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static LifeCycleStatusChoice ToLifeCycleStatusChoice(this LifeCycleStatusType value)
        {
            return DataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped domain value:{value:G}", nameof(value));
        }
    }
}