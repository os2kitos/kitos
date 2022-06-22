using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Users
{
    public class UserRightsAssignments
    {
        public IEnumerable<OrganizationRole> AdministrativeAccessRoles { get; }
        public IEnumerable<DataProcessingRegistrationRight> DataProcessingRegistrationRights { get; }
        public IEnumerable<ItSystemRight> SystemRoles { get; }
        public IEnumerable<ItContractRight> ContractRoles { get; }
        public IEnumerable<ItProjectRight> ProjectRoles { get; }
        public IEnumerable<OrganizationUnitRight> OrganizationUnitRights { get; }

        public UserRightsAssignments(
            IEnumerable<OrganizationRole> organizationRoles,
            IEnumerable<DataProcessingRegistrationRight> dataProcessingRegistrationRoles,
            IEnumerable<ItSystemRight> systemRoles,
            IEnumerable<ItContractRight> contractRoles,
            IEnumerable<ItProjectRight> projectRoles,
            IEnumerable<OrganizationUnitRight> organizationUnitRights)
        {
            AdministrativeAccessRoles = organizationRoles.ToList();
            DataProcessingRegistrationRights = dataProcessingRegistrationRoles.ToList();
            SystemRoles = systemRoles.ToList();
            ContractRoles = contractRoles.ToList();
            ProjectRoles = projectRoles.ToList();
            OrganizationUnitRights = organizationUnitRights.ToList();
        }
    }
}
