using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// It contract role option.
    /// </summary>
    public class ItContractRole : Entity, IRoleEntity<ItContractRight>
    {
        public ItContractRole()
        {
            HasReadAccess = true;
            References = new List<ItContractRight>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItContractRight> References { get; set; }

        /// <summary>
        /// Gets or sets the receivers for an advice.
        /// </summary>
        /// <value>
        /// The receivers for an advice.
        /// </value>
        public virtual ICollection<Advice> ReceiverFor { get; set; }
        /// <summary>
        /// Gets or sets the carbon copy receivers for an advice.
        /// </summary>
        /// <value>
        /// The carbon copy receivers for an advice.
        /// </value>
        public virtual ICollection<Advice> CarbonCopyReceiverFor { get; set; }
    }
}
