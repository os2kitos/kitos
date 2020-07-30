using System;
using System.Collections.Generic;
using Core.DomainModel.Organization;

namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// It contract economy stream.
    /// </summary>
    public class EconomyStream : Entity, IIsPartOfOrganization, IContractModule, IHasAccessModifier
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

        public AccessModifier AccessModifier { get; set; }

        public EconomyStream()
        {
            // Default "Synlighed" must be local
            AccessModifier = AccessModifier.Local;
        }

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            if (ExternPaymentFor != null && ExternPaymentFor.HasUserWriteAccess(user))
                return true;

            if (InternPaymentFor != null && InternPaymentFor.HasUserWriteAccess(user))
                return true;

            return base.HasUserWriteAccess(user);
        }

        public IEnumerable<int> GetOrganizationIds()
        {
            if (ExternPaymentFor != null)
                yield return ExternPaymentFor.OrganizationId;

            if (InternPaymentFor != null)
                yield return InternPaymentFor.OrganizationId;
        }
    }
}
