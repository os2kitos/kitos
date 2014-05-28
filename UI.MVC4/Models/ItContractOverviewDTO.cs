using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;

namespace UI.MVC4.Models
{
    public class ItContractOverviewDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<ItContractOverviewDTO> Children { get; set; }

        public string ResponsibleOrganizationUnitName { get; set; }
        public string SupplierName { get; set; }

        public IEnumerable<RightOutputDTO> Rights { get; set; }

        public string PaymentModelName { get; set; }
        public string PaymentFreqencyName { get; set; }
        
        /// <summary>
        /// The sum of the acquisition column of extern economy streams.
        /// </summary>
        public int AcquisitionSum
        {
            get { return ExternEconomyStreams.Sum(stream => stream.Acquisition); } 
        }

        /// <summary>
        /// The sum of the operation column of extern economy streams.
        /// </summary>
        public int OperationSum 
        { 
            get { return ExternEconomyStreams.Sum(stream => stream.Operation); }
        }

        /// <summary>
        /// The first of the audit dates of extern economy streams.
        /// </summary>
        public DateTime? FirstAuditDate
        {
            get { return ExternEconomyStreams.Min(stream => stream.AuditDate); }
        }

        public int TotalRedStatuses 
        { 
            get { return ExternEconomyStreams.Count(stream => stream.AuditStatus == TrafficLight.Red); }
        }

        public int TotalYellowStatuses
        {
            get { return ExternEconomyStreams.Count(stream => stream.AuditStatus == TrafficLight.Yellow); }
        }

        public int TotalGreenStatuses
        {
            get { return ExternEconomyStreams.Count(stream => stream.AuditStatus == TrafficLight.Green); }
        }

        public IEnumerable<EconomyStreamDTO> ExternEconomyStreams { get; set; }
    }
}