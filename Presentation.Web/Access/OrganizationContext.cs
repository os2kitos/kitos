using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;

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
            int organizationId)
        {
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
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
                if (user.ItSystemRights.Any(x => x.ObjectId == entity.Id && x.Role.HasWriteAccess))
                {
                    result = true;
                }
            }

            return result;
        }
    }
}