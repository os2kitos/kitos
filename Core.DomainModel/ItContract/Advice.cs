using System;

namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// Contains info about Advices on a contract.
    /// </summary>
    public class Advice : Entity
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the contract name.
        /// </summary>
        /// <value>
        /// The contract name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the advice alarm date.
        /// </summary>
        /// <remarks>
        /// Once the alarm expires an email should be sent to all users assigned to 
        /// the <see cref="ItContract"/> with the role defined in <see cref="Receiver"/> 
        /// and <see cref="CarbonCopyReceiver"/>.
        /// </remarks>
        /// <value>
        /// The advice alarm date.
        /// </value>
        public DateTime? AlarmDate { get; set; }

        /// <summary>
        /// Gets or sets the sent date.
        /// </summary>
        /// <value>
        /// The sent date.
        /// </value>
        public DateTime? SentDate { get; set; }

        /// <summary>
        /// Gets or sets the receiver contract role identifier.
        /// </summary>
        /// <remarks>
        /// Contract role id of which to send email.
        /// </remarks>
        /// <value>
        /// The receiver identifier.
        /// </value>
        public int? ReceiverId { get; set; }
        /// <summary>
        /// Gets or sets the receiver contract role.
        /// </summary>
        /// <remarks>
        /// Contract role of which to send email.
        /// </remarks>
        /// <value>
        /// The receiver.
        /// </value>
        public ItContractRole Receiver { get; set; }

        /// <summary>
        /// Gets or sets the carbon copy receiver contract role identifier.
        /// </summary>
        /// <remarks>
        /// Contract role of which to send email.
        /// </remarks>
        /// <value>
        /// The carbon copy receiver identifier.
        /// </value>
        public int? CarbonCopyReceiverId { get; set; }
        /// <summary>
        /// Gets or sets the carbon copy contract role receiver.
        /// </summary>
        /// <remarks>
        /// Contract role of which to send email.
        /// </remarks>
        /// <value>
        /// The carbon copy receiver.
        /// </value>
        public ItContractRole CarbonCopyReceiver { get; set; }

        /// <summary>
        /// Gets or sets the subject of the email.
        /// </summary>
        /// <value>
        /// The email subject.
        /// </value>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets it contract identifier.
        /// </summary>
        /// <value>
        /// It contract identifier.
        /// </value>
        public int ItContractId { get; set; }
        /// <summary>
        /// Gets or sets it contract.
        /// </summary>
        /// <value>
        /// It contract.
        /// </value>
        public virtual ItContract ItContract { get; set; }

        /// <summary>
        /// Determines whether a user has write access to this object.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="organizationId"></param>
        /// <returns>
        /// <c>true</c> if user has write access; otherwise, <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user, int organizationId)
        {
            if (ItContract != null && ItContract.HasUserWriteAccess(user, organizationId)) return true;

            return base.HasUserWriteAccess(user, organizationId);
        }
    }
}
