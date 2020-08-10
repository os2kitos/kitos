﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.Authorization
{
    /// <summary>
    /// Determines the user in a specific organizational context
    /// </summary>
    public class OrganizationalUserContext : IOrganizationalUserContext
    {
        private readonly IReadOnlyDictionary<int, HashSet<OrganizationRole>> _roles;
        private readonly IReadOnlyDictionary<int, OrganizationCategory> _categoriesOfMemberOrganizations;
        private readonly HashSet<OrganizationCategory> _membershipCategories;
        private readonly bool _isGlobalAdmin;

        public OrganizationalUserContext(
            int userId,
            IReadOnlyDictionary<int, IEnumerable<OrganizationRole>> roles,
            IReadOnlyDictionary<int, OrganizationCategory> categoriesOfMemberOrganizations)
        {
            UserId = userId;
            _categoriesOfMemberOrganizations = categoriesOfMemberOrganizations;
            _membershipCategories = new HashSet<OrganizationCategory>(_categoriesOfMemberOrganizations.Values);
            _roles = roles
                .ToDictionary(kvp => kvp.Key, kvp => new HashSet<OrganizationRole>(kvp.Value))
                .AsReadOnly();
            _isGlobalAdmin = _roles.Values.Any(x => x.Contains(OrganizationRole.GlobalAdmin));
        }

        public int UserId { get; }

        public IEnumerable<int> OrganizationIds => _roles.Keys;

        public bool HasRoleInOrganizationOfType(OrganizationCategory category)
        {
            return _membershipCategories.Contains(category);
        }

        public bool IsGlobalAdmin()
        {
            return _isGlobalAdmin;
        }

        public bool HasRole(int organizationId, OrganizationRole role)
        {
            return _roles.TryGetValue(organizationId, out var rolesInOrganization) &&
                   rolesInOrganization.Contains(role);
        }

        public bool HasRoleIn(int organizationId)
        {
            return _roles.ContainsKey(organizationId);
        }

        public bool HasRoleInSameOrganizationAs(IEntity entity)
        {
            switch (entity)
            {
                //Prefer match on hasOrganization first since it has static knowledge of organization relationship
                case IOwnedByOrganization hasOrg:
                    return HasRoleIn(hasOrg.OrganizationId);
                case IIsPartOfOrganization organizationRelationship:
                    return organizationRelationship
                        .GetOrganizationIds()
                        .Any(HasRoleIn);
                default:
                    return false;
            }
        }
    }
}