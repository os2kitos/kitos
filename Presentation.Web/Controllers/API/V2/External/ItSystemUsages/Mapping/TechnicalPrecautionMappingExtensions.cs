using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class TechnicalPrecautionMappingExtensions
    {
        private static readonly IReadOnlyDictionary<TechnicalPrecautionChoice, TechnicalPrecaution> ApiToDataMap;
        private static readonly IReadOnlyDictionary<TechnicalPrecaution, TechnicalPrecautionChoice> DataToApiMap;

        static TechnicalPrecautionMappingExtensions()
        {
            ApiToDataMap = new Dictionary<TechnicalPrecautionChoice, TechnicalPrecaution>
            {
                { TechnicalPrecautionChoice.AccessControl, TechnicalPrecaution.AccessControl },
                { TechnicalPrecautionChoice.Encryption, TechnicalPrecaution.Encryption },
                { TechnicalPrecautionChoice.Logging, TechnicalPrecaution.Logging },
                { TechnicalPrecautionChoice.Pseudonymization, TechnicalPrecaution.Pseudonymization }
            }.AsReadOnly();
            
            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static TechnicalPrecaution ToTechnicalPrecaution(this TechnicalPrecautionChoice value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static TechnicalPrecautionChoice ToTechnicalPrecautionChoice(this TechnicalPrecaution value)
        {
            return DataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped domain value:{value:G}", nameof(value));
        }
    }
}