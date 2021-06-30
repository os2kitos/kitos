﻿using System.Linq;
using Core.DomainModel.Organization;

namespace Core.DomainServices.Queries.User
{
    public class QueryByRoleAssignment : IDomainQuery<DomainModel.User>
    {
        private readonly OrganizationRole _role;

        public QueryByRoleAssignment(OrganizationRole role)
        {
            _role = role;
        }

        public IQueryable<DomainModel.User> Apply(IQueryable<DomainModel.User> users)
        {
            return users.Where(x => x.OrganizationRights.Any(x => x.Role == _role));
        }
    }
}
