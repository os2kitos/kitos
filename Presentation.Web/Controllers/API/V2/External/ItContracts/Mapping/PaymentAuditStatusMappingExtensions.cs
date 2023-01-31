using Core.Abstractions.Types;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Types.Contract;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping
{
    public static class PaymentAuditStatusMappingExtensions
    {
        private static readonly EnumMap<PaymentAuditStatus, TrafficLight> Mapping;
        static PaymentAuditStatusMappingExtensions()
        {
            Mapping = new EnumMap<PaymentAuditStatus, TrafficLight>
            (
                (PaymentAuditStatus.White, TrafficLight.White),
                (PaymentAuditStatus.Red, TrafficLight.Red),
                (PaymentAuditStatus.Yellow, TrafficLight.Yellow),
                (PaymentAuditStatus.Green, TrafficLight.Green)
            );
        }

        public static TrafficLight ToTrafficLight(this PaymentAuditStatus value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static PaymentAuditStatus ToPaymentAuditStatus(this TrafficLight value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}