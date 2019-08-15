using System;
using System.Linq;
using System.Web.DynamicData;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
using NUnit.Framework.Constraints;

namespace Presentation.Web.Access
{
    public class OrganizationContext : AbstractAccessContext
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<ItSystemRole> _systemRoleRepository;
        private readonly int _organizationId;

        public OrganizationContext(
            IGenericRepository<User> userRepository, 
            IGenericRepository<Organization> organizationRepository, 
            IGenericRepository<ItSystemRole> systemRoleRepository,
            int organizationId)
        {
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _systemRoleRepository = systemRoleRepository;
            _organizationId = organizationId;
        }

        public override bool AllowReads(int userId)
        {
            var result = false;

            var user = _userRepository.GetByKey(userId);
            if (user.IsGlobalAdmin)
            {
                result = true;
            }
            else if (user.DefaultOrganizationId == _organizationId)
            {
                result = true;
            }

            var organization = _organizationRepository.GetByKey(_organizationId);
            if (organization.AccessModifier == AccessModifier.Public)
            {
                result = true;
            }

            return result;
        }

        public override bool AllowReads(int userId, ItSystem entity)
        {
            var result = false;

            var user = _userRepository.GetByKey(userId);
            if (user.IsGlobalAdmin)
            {
                result = true;
            }
            else if (user.DefaultOrganizationId == _organizationId)
            {
                result = true;
            }
            else if (entity.AccessModifier == AccessModifier.Public)
            {
                result = true;
            }

            return result;
        }

        public override bool AllowUpdates(int userId, ItSystemUsage entity)
        {
            var result = false;

            var user = _userRepository.GetByKey(userId);
            if (user.IsGlobalAdmin)
            {
                result = true;
            }
            else if (user.DefaultOrganizationId == _organizationId && user.IsLocalAdmin)
            {
                result = true;
            }
            else
            {
                var itSystemRolesWithWriteAccess = _systemRoleRepository.AsQueryable().Where(role => role.HasWriteAccess);
                var itSystemUsersWithWriteAccess =
                    from right in entity.Rights
                    from role in itSystemRolesWithWriteAccess
                    where right.RoleId.Equals(role.Id)
                    where right.UserId.Equals(userId)
                    select new {right.UserId};
                if (itSystemUsersWithWriteAccess.Any())
                {
                    result = true;
                }
            }

            return result;
        }
    }
}