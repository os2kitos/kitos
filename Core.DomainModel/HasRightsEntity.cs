using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel
{
    /// <summary>
    /// Entity which holds a list of rights
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TRight">The type of the right.</typeparam>
    /// <typeparam name="TRole">The type of the role.</typeparam>
    public abstract class HasRightsEntity<TModel, TRight, TRole> : Entity
        where TModel : HasRightsEntity<TModel, TRight, TRole>
        where TRight : IRight<TModel, TRight, TRole>
        where TRole : IRoleEntity<TRight>
    {
        protected HasRightsEntity()
        {
            this.Rights = new List<TRight>();
        }

        public virtual ICollection<TRight> Rights { get; set; }

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access; otherwise, <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            if (Rights.Any(right => right.UserId == user.Id && right.Role.HasWriteAccess)) return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
