using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Types.Contract;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping
{
    public static class PaymentAuditStatusMappingExtensions
    {
        private static readonly IReadOnlyDictionary<PaymentAuditStatus, TrafficLight> ApiToDataMap;
        private static readonly IReadOnlyDictionary<TrafficLight, PaymentAuditStatus> DataToApiMap;

        static PaymentAuditStatusMappingExtensions()
        {
            ApiToDataMap = new Dictionary<PaymentAuditStatus, TrafficLight>
            {
                { PaymentAuditStatus.White, TrafficLight.White },
                { PaymentAuditStatus.Red, TrafficLight.Red },
                { PaymentAuditStatus.Yellow, TrafficLight.Yellow },
                { PaymentAuditStatus.Green, TrafficLight.Green }
            }.AsReadOnly();
            
            DataToApiMap = ApiToDataMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public static TrafficLight ToTrafficLight(this PaymentAuditStatus value)
        {
            return ApiToDataMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped choice:{value:G}", nameof(value));
        }

        public static PaymentAuditStatus ToPaymentAuditStatus(this TrafficLight value)
        {
            return DataToApiMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped domain value:{value:G}", nameof(value));
        }
    }
}