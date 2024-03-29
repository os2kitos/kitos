﻿using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.Organization;


namespace Core.DomainServices
{
    public interface IUserRepository : IGenericRepository<User>
    {
        IQueryable<User> GetGlobalAdmins();
        User GetByEmail(string email);
        User GetById(int id);
        IQueryable<User> SearchOrganizationUsers(int organizationId, Maybe<string> query);
        IQueryable<User> GetUsers();
        IQueryable<User> GetUsersWithCrossOrganizationPermissions();
        IQueryable<User> GetUsersWithRoleAssignment(OrganizationRole role);
        IQueryable<User> GetUsersInOrganization(int organizationId);
        Maybe<User> GetByUuid(Guid uuid);
    }
}