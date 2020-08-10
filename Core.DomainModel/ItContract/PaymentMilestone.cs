using System;

namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// It contract payment milestone
    /// </summary>
    public class PaymentMilestone : Entity, IContractModule
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets the expected date.
        /// </summary>
        /// <value>
        /// The expected date.
        /// </value>
        public DateTime? Expected { get; set; }
        /// <summary>
        /// Gets or sets the approved date.
        /// </summary>
        /// <value>
        /// The approved date.
        /// </value>
        public DateTime? Approved { get; set; }
        /// <summary>
        /// Gets or sets the associated it contract identifier.
        /// </summary>
        /// <value>
        /// It contract identifier.
        /// </value>
        public int ItContractId { get; set; }
        /// <summary>
        /// Gets or sets the associated it contract.
        /// </summary>
        /// <value>
        /// It contract.
        /// </value>
        public virtual ItContract ItContract { get; set; }


        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            if (ItContract != null && ItContract.HasUserWriteAccess(user))
                return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
