using System;
using Core.DomainModel;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractPayment
    {
        public Guid? OrganizationUnitUuid { get; set; }
        public int Acquisition { get; set; }
        public int Operation { get; set; }
        public int Other { get; set; }
        public required string AccountingEntry { get; set; }
        public TrafficLight AuditStatus { get; set; }
        public DateTime? AuditDate { get; set; }
        public required string Note { get; set; }
    }
}
