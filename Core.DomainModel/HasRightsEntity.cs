using System;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystem;

// ReSharper disable VirtualMemberCallInConstructor

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
        where TRole : IRoleEntity, IHasId
    {
        protected HasRightsEntity()
        {
            Rights = new List<TRight>();
        }

        public virtual ICollection<TRight> Rights { get; set; }

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            // check if the user has a write role on this instance
            return Rights.Any(right => right.UserId == user.Id && right.Role.HasWriteAccess) || base.HasUserWriteAccess(user);
        }
        

        public IEnumerable<TRight> GetRights(int roleId)
        {
            return Rights.Where(x => x.RoleId == roleId);
        }

        public abstract TRight CreateNewRight(TRole role, User user);

        public Result<TRight, OperationError> AssignRole(TRole role, User user)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (HasRight(role, user))
                return new OperationError("Existing right for same role found for the same user", OperationFailure.Conflict);

            var newRight = CreateNewRight(role, user);

            Rights.Add(newRight);

            return newRight;
        }

        public Result<TRight, OperationError> RemoveRole(TRole role, User user)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (user == null) throw new ArgumentNullException(nameof(user));

            return GetRight(role, user)
                .Match<Result<TRight, OperationError>>
                (
                    right =>
                    {
                        Rights.Remove(right);
                        return right;
                    },
                    () => new OperationError($"Role with id {role.Id} is not assigned to user with id ${user.Id}",
                        OperationFailure.BadInput)
                );

        }

        public void ResetRoles()
        {
            Rights.Clear();
        }

        private bool HasRight(TRole role, User user)
        {
            return GetRight(role, user).HasValue;
        }

        private Maybe<TRight> GetRight(TRole role, User user)
        {
            return GetRights(role.Id).FirstOrDefault(x => x.UserId == user.Id);
        }
    }
}
