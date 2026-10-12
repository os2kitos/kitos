using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V2.Types.KendoGrid;
using Presentation.Web.Models.API.V2.Types.Notifications;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping;

public static class OverviewTypeMappingExtensions
{
    private static readonly EnumMap<OverviewType, OverviewTypeOptions> Mapping;

    static OverviewTypeMappingExtensions()
    {
        Mapping = new EnumMap<OverviewType, OverviewTypeOptions>
        (
            (OverviewType.ItContract, OverviewTypeOptions.ItContract),
            (OverviewType.ItSystemUsage, OverviewTypeOptions.ItSystemUsage),
            (OverviewType.DataProcessingRegistration, OverviewTypeOptions.DataProcessingRegistration)
        );
    }

    public static OverviewTypeOptions ToOptions(this OverviewType value)
    {
        return Mapping.FromLeftToRight(value);
    }

    public static OverviewType ToDomain(this OverviewTypeOptions value)
    {
        return Mapping.FromRightToLeft(value);
    }
}