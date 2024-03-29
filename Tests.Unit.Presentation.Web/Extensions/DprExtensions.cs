﻿using Core.DomainModel.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;

namespace Tests.Unit.Presentation.Web.Extensions
{
    public static class DprExtensions
    {
        public static DataProcessingRegistration WithDataProcessor(this DataProcessingRegistration dpr, Organization dataProcessor)
        {
            dpr.AssignDataProcessor(dataProcessor);
            dataProcessor.DataProcessorForDataProcessingRegistrations.Add(dpr);
            return dpr;
        }

        public static DataProcessingRegistration WithSubDataProcessor(this DataProcessingRegistration dpr, Organization subDataProcessor)
        {
            dpr.HasSubDataProcessors = YesNoUndecidedOption.Yes;
            var dataProcessor = dpr.AssignSubDataProcessor(subDataProcessor);
            subDataProcessor.SubDataProcessorRegistrations.Add(dataProcessor.Value);
            return dpr;
        }
    }
}
