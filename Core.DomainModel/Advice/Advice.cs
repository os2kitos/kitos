using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Shared;

namespace Core.DomainModel.Advice
{
    public enum Scheduling
    {
        Immediate = 0,
        Hour = 1,
        Day = 2,
        Week = 3,
        Month = 4,
        Year = 5,
        Quarter = 6,
        Semiannual = 7
    }

    public enum AdviceType
    {
        Immediate = 0,
        Repeat = 1
    }

    /// <summary>
    /// Contains info about Advices on a contract.
    /// </summary>
    public class Advice : Entity, ISystemModule, IContractModule, IHasUuid
    {
        public Advice()
        {
            AdviceSent = new List<AdviceSent>();
            Reciepients = new List<AdviceUserRelation>();
            Uuid = Guid.NewGuid();
        }

        public static string CreateJobId(int adviceId)
        {
            return $"Advice: {adviceId}";
        }
        public Guid Uuid { get; set; }

        public virtual ICollection<AdviceSent> AdviceSent { get; set; }
        public virtual ICollection<AdviceUserRelation> Reciepients { get; set; }
        public int? RelationId { get; set; }
        public RelatedEntityType Type { get; set; }

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
        /// the <see cref="Core.DomainModel.ItContract"/> with the role defined in <see cref="Receiver"/>
        /// and <see cref="CarbonCopyReceiver"/>.
        /// </remarks>
        /// <value>
        /// The advice alarm date.
        /// </value>
        public DateTime? AlarmDate { get; set; }


        /// <summary>
        /// Gets or sets the stop date.
        /// </summary>
        /// <value>
        /// The stop date.
        /// </value>
        public DateTime? StopDate { get; set; }

        /// <summary>
        /// Gets or sets the sent date.
        /// </summary>
        /// <value>
        /// The sent date.
        /// </value>
        public DateTime? SentDate { get; set; }

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
        /// Gets or sets the job id.
        /// </summary>
        public string JobId { get; set; }

        /// <summary>
        /// Gets or sets the advice type
        /// </summary>
        public AdviceType AdviceType { get; set; }

        /// <summary>
        ///     Whether the advice can be deleted
        ///     Is false if the advice has been sent at least once or if it is active.
        /// </summary>
        public bool CanBeDeleted
        {
            get
            {
                if (AdviceSent.Any())
                {
                    return false;
                }
                return IsActive == IsType(AdviceType.Immediate);
            }
        }

        public bool HasInvalidState()
        {
            return ObjectOwnerId == null || RelationId == null;
        }

        public static string CreatePartitionJobId(int adviceId, int partition)
        {
            return $"{CreateJobId(adviceId)}_part_{partition}";
        }

        public bool IsType(AdviceType type)
        {
            return AdviceType == type;
        }
    }
}
