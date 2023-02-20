using Core.Abstractions.Types;
using Core.DomainModel.Advice;
using Presentation.Web.Models.API.V2.Types.Notifications;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public static class RepetitionFrequencyMappingExtensions
    {
        private static readonly EnumMap<RepetitionFrequencyOptions, Scheduling> Mapping;

        static RepetitionFrequencyMappingExtensions()
        {
            Mapping = new EnumMap<RepetitionFrequencyOptions, Scheduling>
            (
                (RepetitionFrequencyOptions.Hour, Scheduling.Hour),
                (RepetitionFrequencyOptions.Day, Scheduling.Day),
                (RepetitionFrequencyOptions.Week, Scheduling.Week),
                (RepetitionFrequencyOptions.Month, Scheduling.Month),
                (RepetitionFrequencyOptions.Quarter, Scheduling.Quarter),
                (RepetitionFrequencyOptions.HalfYear, Scheduling.Semiannual),
                (RepetitionFrequencyOptions.Year, Scheduling.Year)
            );
        }

        public static RepetitionFrequencyOptions? ToRepetitionFrequencyOptions(this Scheduling value)
        {
            if (value == Scheduling.Immediate)
                return null;

            return Mapping.FromRightToLeft(value);
        }

        public static Scheduling ToScheduling(this RepetitionFrequencyOptions value)
        {
            return Mapping.FromLeftToRight(value);
        }
    }
}