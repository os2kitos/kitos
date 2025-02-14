using Core.Abstractions.Types;
using Core.DomainModel.ItSystem.DataTypes;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class HostedAtMappingExtensions
    {
        private static readonly EnumMap<HostingChoice, HostedAt> Mapping;

        static HostedAtMappingExtensions()
        {
            Mapping = new EnumMap<HostingChoice, HostedAt>
            (
                (HostingChoice.External, HostedAt.EXTERNAL),
                (HostingChoice.OnPremise, HostedAt.ONPREMISE),
                (HostingChoice.Undecided, HostedAt.UNDECIDED),
                (HostingChoice.Hybrid, HostedAt.HYBRID)
            );
        }

        public static HostedAt ToHostedAt(this HostingChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static HostingChoice ToHostingChoice(this HostedAt value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}