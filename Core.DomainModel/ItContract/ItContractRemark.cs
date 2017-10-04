namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// The it contract notes class.
    /// </summary>
    public class ItContractRemark : Entity, IContextAware, IContractModule
    {

        public virtual ItContract ItContract { get; set; }
        /// <summary>
        /// Gets or sets the remark of a contract.
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            // If the user has write access to the contract the user should also have write access to the remark
            if (ItContract != null && ItContract.HasUserWriteAccess(user))
                return true;

            return base.HasUserWriteAccess(user);
        }

        /// <summary>
        /// Determines whether this instance is within a given organizational context.
        /// </summary>
        /// <param name="organizationId">The organization identifier (context) the user is accessing from.</param>
        /// <returns>
        ///   <c>true</c> if this instance is in the organizational context, otherwise <c>false</c>.
        /// </returns>
        public bool IsInContext(int organizationId)
        {
            if (ItContract != null)
                return ItContract.IsInContext(organizationId);

            return false;
        }
    }
}
