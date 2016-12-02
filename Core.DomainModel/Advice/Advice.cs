using System;
using System.Collections.Generic;

namespace Core.DomainModel.Advice
{
    public enum ObjectType
    {
        Contract,
        Itsytem
    }
    public enum Scheduling
    {
        Monthly,
        Daily
    }
    /// <summary>
    /// Contains info about Advices on a contract.
    /// </summary>
    public class Advice : Entity, IContextAware
    {
        public Advice() {
            AdviceSent = new List<AdviceSent.AdviceSent>();
        }

        public virtual ICollection<AdviceSent.AdviceSent> AdviceSent { get; set; }
        public int? RelationId { get; set; }
        public ObjectType? Type { get; set; }
        public Scheduling? Scheduling { get; set; }
       
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
        public IRoleEntity Receiver { get; set; }

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
        public IRoleEntity CarbonCopyReceiver { get; set; }
        /// <summary>
        /// Gets or sets the body of the email.
        /// </summary>
        /// <value>
        /// The email body.
        /// </value>
        public string Body { get; set; }
        
        /// <summary>
        /// Gets or sets the subject of the email.
        /// </summary>
        /// <value>
        /// The email subject.
        /// </value>
        public string Subject { get; set; }
        

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
           /* if (Object != null && Object.HasUserWriteAccess(user))
                return true;
                */
            return base.HasUserWriteAccess(user);
        }

        public bool IsInContext(int organizationId)
        {
            throw new NotImplementedException();
        }

        /*/// <summary>
        /// Determines whether this instance is within a given organizational context.
        /// </summary>
        /// <param name="organizationId">The organization identifier (context) the user is accessing from.</param>
        /// <returns>
        ///   <c>true</c> if this instance is in the organizational context, otherwise <c>false</c>.
        /// </returns>
        public bool IsInContext(int organizationId)
        {
            if (Object != null)
                return Object.IsInContext(organizationId);

            return false;
        }*/
    }
}
