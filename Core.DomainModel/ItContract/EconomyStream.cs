using System;
using System.Collections.Generic;
using System.IO;
using Core.DomainModel.Organization;

namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// It contract economy stream.
    /// </summary>
    public class EconomyStream : Entity, IIsPartOfOrganization, IContractModule, ISupportsUserSpecificAccessControl
    {
        /// <summary>
        /// The EconomyStream might be an extern payment for a contract.
        /// </summary>
        /// <value>
        /// The extern payment for.
        /// </value>
        public virtual ItContract ExternPaymentFor { get; set; }
        public int? ExternPaymentForId { get; set; }

        /// <summary>
        /// The EconomyStream might be an intern payment for a contract.
        /// </summary>
        public virtual ItContract InternPaymentFor { get; set; }
        public int? InternPaymentForId { get; set; }

        /// <summary>
        /// Gets or sets the associated organization unit identifier.
        /// </summary>
        /// <remarks>
        /// This is the economy stream for the set organization unit.
        /// </remarks>
        /// <value>
        /// The organization unit identifier.
        /// </value>
        public int? OrganizationUnitId { get; set; }
        /// <summary>
        /// Gets or sets the organization unit.
        /// </summary>
        /// <remarks>
        /// This is the economy stream for the set organization unit.
        /// </remarks>
        /// <value>
        /// The organization unit.
        /// </value>
        public virtual OrganizationUnit OrganizationUnit { get; set; }

        /// <summary>
        /// The field "anskaffelse".
        /// </summary>
        /// <value>
        /// The acquisition.
        /// </value>
        public int Acquisition { get; set; }

        /// <summary>
        /// The field "drift/år".
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        public int Operation { get; set; }

        public int Other { get; set; }

        /// <summary>
        /// The field "kontering".
        /// </summary>
        /// <value>
        /// The accounting entry.
        /// </value>
        public string AccountingEntry { get; set; }

        /// <summary>
        /// Traffic light for audit.
        /// </summary>
        /// <value>
        /// The audit status.
        /// </value>
        public TrafficLight AuditStatus { get; set; }

        /// <summary>
        /// DateTime for audit.
        /// </summary>
        /// <value>
        /// The audit date.
        /// </value>
        public DateTime? AuditDate { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        public string Note { get; set; }

        public EconomyStream()
        {
            
        }

        public static EconomyStream CreateInternalEconomyStream(ItContract contract, OrganizationUnit optionalOrganizationUnit, int acquisition, int operation, int other, string accountingEntry, TrafficLight auditStatus, DateTime? auditDate, string note)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            var economyStream = CreateEconomyStreamWithoutDirection(optionalOrganizationUnit, acquisition, operation, other, accountingEntry, auditStatus, auditDate, note);
            economyStream.InternPaymentFor = contract;
            return economyStream;
        }

        public static EconomyStream CreateExternalEconomyStream(ItContract contract, OrganizationUnit optionalOrganizationUnit, int acquisition, int operation, int other, string accountingEntry, TrafficLight auditStatus, DateTime? auditDate, string note)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            var economyStream = CreateEconomyStreamWithoutDirection(optionalOrganizationUnit, acquisition, operation, other, accountingEntry, auditStatus, auditDate, note);
            economyStream.ExternPaymentFor = contract;
            return economyStream;
        }

        private static EconomyStream CreateEconomyStreamWithoutDirection(OrganizationUnit optionalOrganizationUnit, int acquisition, int operation, int other, string accountingEntry, TrafficLight auditStatus, DateTime? auditDate, string note)
        {
            return new EconomyStream
            {
                OrganizationUnit = optionalOrganizationUnit,
                Acquisition = acquisition,
                Note = note,
                AuditDate = auditDate?.Date,
                AuditStatus = auditStatus,
                Operation = operation,
                Other = other,
                AccountingEntry = accountingEntry
            };
        }

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        public bool HasUserWriteAccess(User user)
        {
            if (ExternPaymentFor != null && ExternPaymentFor.HasUserWriteAccess(user))
                return true;

            if (InternPaymentFor != null && InternPaymentFor.HasUserWriteAccess(user))
                return true;

            return false;
        }

        public IEnumerable<int> GetOrganizationIds()
        {
            if (ExternPaymentFor != null)
                yield return ExternPaymentFor.OrganizationId;

            if (InternPaymentFor != null)
                yield return InternPaymentFor.OrganizationId;
        }

        public void SetOrganizationUnit(OrganizationUnit unit)
        {
            OrganizationUnit = unit;
            //OrganizationUnitId = unit.Id;
        }

        public void ResetOrganizationUnit()
        {
            OrganizationUnit = null;
            //OrganizationUnitId = null;
        }
    }
}
