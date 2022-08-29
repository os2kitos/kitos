using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Users
{
    public class UserRightsAssignments
    {
        public IEnumerable<OrganizationRole> LocalAdministrativeAccessRoles { get; }
        public IEnumerable<DataProcessingRegistrationRight> DataProcessingRegistrationRights { get; }
        public IEnumerable<ItSystemRight> SystemRights { get; }
        public IEnumerable<ItContractRight> ContractRights { get; }
        public IEnumerable<OrganizationUnitRight> OrganizationUnitRights { get; }

        public UserRightsAssignments(
            IEnumerable<OrganizationRole> organizationRoles,
            IEnumerable<DataProcessingRegistrationRight> dataProcessingRegistrationRoles,
            IEnumerable<ItSystemRight> systemRights,
            IEnumerable<ItContractRight> contractRights,
            IEnumerable<OrganizationUnitRight> organizationUnitRights)
        {
            LocalAdministrativeAccessRoles = organizationRoles.ToList();
            DataProcessingRegistrationRights = dataProcessingRegistrationRoles.ToList();
            SystemRights = systemRights.ToList();
            ContractRights = contractRights.ToList();
            OrganizationUnitRights = organizationUnitRights.ToList();
        }
    }
}
