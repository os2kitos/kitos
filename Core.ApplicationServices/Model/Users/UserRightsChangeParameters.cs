using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Users
{
    public class UserRightsChangeParameters
    {
        public IEnumerable<OrganizationRole> AdministrativeAccessRoles { get; }
        public IEnumerable<int> DataProcessingRegistrationRightIds { get; }
        public IEnumerable<int> SystemRightIds { get; }
        public IEnumerable<int> ContractRightIds { get; }
        public IEnumerable<int> ProjectRightIds { get; }
        public IEnumerable<int> OrganizationUnitRightsIds { get; }

        public UserRightsChangeParameters(
            IEnumerable<OrganizationRole> organizationRoles,
            IEnumerable<int> dataProcessingRegistrationRoles,
            IEnumerable<int> systemRoles,
            IEnumerable<int> contractRoles,
            IEnumerable<int> projectRoles,
            IEnumerable<int> organizationUnitRights)
        {
            AdministrativeAccessRoles = organizationRoles.ToList();
            DataProcessingRegistrationRightIds = dataProcessingRegistrationRoles.ToList();
            SystemRightIds = systemRoles.ToList();
            ContractRightIds = contractRoles.ToList();
            ProjectRightIds = projectRoles.ToList();
            OrganizationUnitRightsIds = organizationUnitRights.ToList();
        }
    }
}
