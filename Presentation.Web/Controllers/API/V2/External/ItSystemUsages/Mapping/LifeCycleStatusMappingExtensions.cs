using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class LifeCycleStatusMappingExtensions
    {
        private static readonly EnumMap<LifeCycleStatusChoice, LifeCycleStatusType> Mapping;

        static LifeCycleStatusMappingExtensions()
        {
            Mapping = new EnumMap<LifeCycleStatusChoice, LifeCycleStatusType>
            (
                (LifeCycleStatusChoice.Undecided, LifeCycleStatusType.Undecided),
                (LifeCycleStatusChoice.NotInUse, LifeCycleStatusType.NotInUse),
                (LifeCycleStatusChoice.PhasingIn, LifeCycleStatusType.PhasingIn),
                (LifeCycleStatusChoice.Operational, LifeCycleStatusType.Operational)
            );
        }

        public static LifeCycleStatusType ToLifeCycleStatusType(this LifeCycleStatusChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static LifeCycleStatusChoice ToLifeCycleStatusChoice(this LifeCycleStatusType value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}