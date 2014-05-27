using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItContract
{
    public class EconomyStream : IEntity<int>
    {
        public int Id { get; set; }

        /// <summary>
        /// The EconomyStream might be an extern payment for a contract
        /// </summary>
        public ItContract ExternPaymentFor { get; set; }
        public int? ExternPaymentForId { get; set; }

        /// <summary>
        /// The EconomyStream might be an intern payment for a contract
        /// </summary>
        public ItContract InternPaymentFor { get; set; }
        public int? InternPaymentForId { get; set; }

        public int? OrganizationUnitId { get; set; }
        public virtual OrganizationUnit OrganizationUnit { get; set; }

        /// <summary>
        /// The field "anskaffelse"
        /// </summary>
        public int Acquisition { get; set; }

        /// <summary>
        /// The field "drift/år"
        /// </summary>
        public int Operation { get; set; }

        public int Other { get; set; }
        
        /// <summary>
        /// The field "kontering"
        /// </summary>
        public string AccountingEntry { get; set; }

        /// <summary>
        /// Traffic light for audit
        /// </summary>
        public int AuditStatus { get; set; }

        /// <summary>
        /// DateTime for audit
        /// </summary>
        public DateTime? AuditDate { get; set; }

        public string Note { get; set; }
        }
}
