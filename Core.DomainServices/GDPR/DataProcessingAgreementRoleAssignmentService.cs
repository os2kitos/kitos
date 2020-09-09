using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainServices.Extensions;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingAgreementRoleAssignmentService : IDataProcessingAgreementRoleAssignmentService
    {
        public IQueryable<User> GetUsersWhichCanBeAssignedToRole(DataProcessingAgreement target, DataProcessingAgreementRole role, IQueryable<User> candidates)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));

            var usersWithSameRole = GetIdsOfUsersAssignedToRole(target, role);
            return candidates.ExceptEntitiesWithIds(usersWithSameRole.ToList());
        }

        private static IEnumerable<int> GetIdsOfUsersAssignedToRole(DataProcessingAgreement target, DataProcessingAgreementRole role)
        {
            return target.GetRights(role.Id).Select(x => x.UserId).Distinct();
        }
    }
}
