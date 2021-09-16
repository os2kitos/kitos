using System;
using Core.DomainModel.ItContract;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractTerminationTerms
    {
        public Guid? NoticePeriodMonthsUuid { get; set; }
        public YearSegmentOption? NoticePeriodExtendsCurrent { get; set; }
        public YearSegmentOption? NoticeByEndOf { get; set; }
    }
}