using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;

namespace Presentation.Web.Models
{
    public class ItContractOverviewDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<ItContractOverviewDTO> Children { get; set; }

        public string ResponsibleOrganizationUnitName { get; set; }
        public string SupplierName { get; set; }
        public string ContractSignerName { get; set; }
        public string ContractSignerLastName { get; set; }

        public string ContractSignerFullName
        {
            get { return ContractSignerName + " " + ContractSignerLastName; }
        }

        public IEnumerable<RightOutputDTO> Rights { get; set; }

        public string PaymentModelName { get; set; }

        /// <summary>
        /// When the contract began (indgået)
        /// </summary>
        public DateTime? Concluded { get; set; }
        /// <summary>
        /// When the contract expires (udløbet)
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// When the contract ends (opsagt)
        /// </summary>
        public DateTime? Terminated { get; set; }

        /// <summary>
        /// Whether the contract is active or not
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The sum of the acquisition column of extern economy streams.
        /// </summary>
        public int? AcquisitionSum
        {
            get
            {
                if (!ExternEconomyStreams.Any()) return null;

                return ExternEconomyStreams.Sum(stream => stream.Acquisition);
            }
        }

        /// <summary>
        /// The sum of the operation column of extern economy streams.
        /// </summary>
        public int? OperationSum
        {
            get
            {
                if (!ExternEconomyStreams.Any()) return null;

                return ExternEconomyStreams.Sum(stream => stream.Operation);
            }
        }

        /// <summary>
        /// The first of the audit dates of extern economy streams.
        /// </summary>
        public DateTime? FirstAuditDate
        {
            get { return ExternEconomyStreams.Min(stream => stream.AuditDate); }
        }

        /// <summary>
        /// The number of extern economy streams, that have white status
        /// </summary>
        public int TotalWhiteStatuses
        {
            get { return ExternEconomyStreams.Count(stream => stream.AuditStatus == TrafficLight.White); }
        }

        /// <summary>
        /// The number of extern economy streams, that have red status
        /// </summary>
        public int TotalRedStatuses
        {
            get { return ExternEconomyStreams.Count(stream => stream.AuditStatus == TrafficLight.Red); }
        }

        /// <summary>
        /// The number of extern economy streams, that have yellow status
        /// </summary>
        public int TotalYellowStatuses
        {
            get { return ExternEconomyStreams.Count(stream => stream.AuditStatus == TrafficLight.Yellow); }
        }

        /// <summary>
        /// The number of extern economy streams, that have green status
        /// </summary>
        public int TotalGreenStatuses
        {
            get { return ExternEconomyStreams.Count(stream => stream.AuditStatus == TrafficLight.Green); }
        }

        public IEnumerable<EconomyStreamDTO> ExternEconomyStreams { get; set; }
    }
}
