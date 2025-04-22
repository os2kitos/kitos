using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared.Write;
using Core.DomainModel.ItContract;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices.GDPR;

namespace Core.ApplicationServices.Helpers
{
    public class RoleMappingHelper
    {
        public static IReadOnlyList<UserRolePair> ExtractAssignedRoles<TModel, TRight, TRole>(HasRightsEntity<TModel, TRight, TRole> entity)
            where TModel : HasRightsEntity<TModel, TRight, TRole>
            where TRight : IRight<TModel, TRight, TRole>
            where TRole : IRoleEntity, IHasId
        {
            return entity.Rights.Select(right => new UserRolePair(right.User.Uuid, right.Role.Uuid)).ToList();
        }
    }
}
