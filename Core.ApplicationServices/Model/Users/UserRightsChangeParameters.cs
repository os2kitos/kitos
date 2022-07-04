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
            IEnumerable<int> dataProcessingRegistrationRightIds,
            IEnumerable<int> systemRightIds,
            IEnumerable<int> contractRightIds,
            IEnumerable<int> projectRightIds,
            IEnumerable<int> organizationUnitRightIds)
        {
            AdministrativeAccessRoles = organizationRoles.ToHashSet();
            DataProcessingRegistrationRightIds = dataProcessingRegistrationRightIds.ToHashSet();
            SystemRightIds = systemRightIds.ToHashSet();
            ContractRightIds = contractRightIds.ToHashSet();
            ProjectRightIds = projectRightIds.ToHashSet();
            OrganizationUnitRightsIds = organizationUnitRightIds.ToHashSet();
        }
    }
}
