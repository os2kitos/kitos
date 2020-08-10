using System;

namespace Core.DomainModel.ItContract
{
    public class HandoverTrial : Entity, IContractModule
    {
        public DateTime? Expected { get; set; }
        public DateTime? Approved { get; set; }
        public int ItContractId { get; set; }
        public virtual ItContract ItContract { get; set; }

        public int? HandoverTrialTypeId { get; set; }
        public virtual HandoverTrialType HandoverTrialType { get; set; }

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
