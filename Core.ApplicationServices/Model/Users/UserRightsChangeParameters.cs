using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Users
{
    public class UserRightsChangeParameters
    {
        public ISet<OrganizationRole> AdministrativeAccessRoles { get; }
        public ISet<int> DataProcessingRegistrationRightIds { get; }
        public ISet<int> SystemRightIds { get; }
        public ISet<int> ContractRightIds { get; }
        public ISet<int> ProjectRightIds { get; }
        public ISet<int> OrganizationUnitRightsIds { get; }

        public UserRightsChangeParameters(
            IEnumerable<OrganizationRole> organizationRoles,
            IEnumerable<int> dataProcessingRegistrationRoles,
            IEnumerable<int> systemRoles,
            IEnumerable<int> contractRoles,
            IEnumerable<int> projectRoles,
            IEnumerable<int> organizationUnitRights)
        {
            AdministrativeAccessRoles = organizationRoles.ToHashSet();
            DataProcessingRegistrationRightIds = dataProcessingRegistrationRoles.ToHashSet();
            SystemRightIds = systemRoles.ToHashSet();
            ContractRightIds = contractRoles.ToHashSet();
            ProjectRightIds = projectRoles.ToHashSet();
            OrganizationUnitRightsIds = organizationUnitRights.ToHashSet();
        }
    }
}
