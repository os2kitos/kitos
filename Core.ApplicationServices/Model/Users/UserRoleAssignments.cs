using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Users
{
    public class UserRoleAssignments
    {
        public IEnumerable<OrganizationRole> AdministrativeAccessRoles { get; }
        public IEnumerable<DataProcessingRegistrationRight> DataProcessingRegistrationRoles { get; }
        public IEnumerable<ItSystemRight> SystemRoles { get; }
        public IEnumerable<ItContractRight> ContractRoles { get; }
        public IEnumerable<ItProjectRight> ProjectRoles { get; }
        public IEnumerable<OrganizationUnitRight> OrganizationUnitRights { get; }

        public UserRoleAssignments(
            IEnumerable<OrganizationRole> organizationRoles,
            IEnumerable<DataProcessingRegistrationRight> dataProcessingRegistrationRoles,
            IEnumerable<ItSystemRight> systemRoles,
            IEnumerable<ItContractRight> contractRoles,
            IEnumerable<ItProjectRight> projectRoles,
            IEnumerable<OrganizationUnitRight> organizationUnitRights)
        {
            AdministrativeAccessRoles = organizationRoles.ToList();
            DataProcessingRegistrationRoles = dataProcessingRegistrationRoles.ToList();
            SystemRoles = systemRoles.ToList();
            ContractRoles = contractRoles.ToList();
            ProjectRoles = projectRoles.ToList();
            OrganizationUnitRights = organizationUnitRights.ToList();
        }
    }
}
