using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel
{
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

        public override bool HasUserWriteAccess(User user)
        {
            if (Rights.Any(right => right.Role.HasWriteAccess)) return true;

            return base.HasUserWriteAccess(user);
        }
    }
}