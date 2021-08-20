using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Result;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.Mapping
{
    public static class ArchiveDutyMappingExtensions
    {
        private static readonly IReadOnlyDictionary<ArchiveDutyChoice, ArchiveDutyTypes> ApiToDataMap;
        private static readonly IReadOnlyDictionary<ArchiveDutyTypes, ArchiveDutyChoice> DataToApiMap;

        static ArchiveDutyMappingExtensions()
        {
            ApiToDataMap = new Dictionary<ArchiveDutyChoice, ArchiveDutyTypes>()
            {
                { ArchiveDutyChoice.B ,ArchiveDutyTypes.B},
                { ArchiveDutyChoice.K ,ArchiveDutyTypes.K},
                { ArchiveDutyChoice.Undecided ,ArchiveDutyTypes.Undecided},
                { ArchiveDutyChoice.Unknown ,ArchiveDutyTypes.Unknown}
            }.AsReadOnly();
            DataToApiMap = ApiToDataMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key).AsReadOnly();
        }

        public static ArchiveDutyTypes ToArchiveDutyTypes(this ArchiveDutyChoice value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static ArchiveDutyChoice ToArchiveDutyChoice(this ArchiveDutyTypes value)
        {
            return DataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped domain value:{value:G}", nameof(value));
        }
    }
}