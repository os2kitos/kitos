using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Role;

namespace Core.ApplicationServices.UIConfiguration.Handlers
{
    public class HandleUserBeingDeleted : IDomainEventHandler<EntityBeingDeletedEvent<User>>
    {
        private readonly IDataProcessingRegistrationApplicationService _dataProcessingRegistrationApplicationService;
        private readonly IOrganizationRightsService _organizationRightsService;
        private readonly IRoleAssignmentService<ItContractRight, ItContractRole, ItContract> _itContractRightService;
        private readonly IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage> _itSystemRightService;
        private readonly IRoleAssignmentService<ItProjectRight, ItProjectRole, ItProject> _itProjectRightService;
        private readonly IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit> _organizationUnitRightService;


        public HandleUserBeingDeleted(IDataProcessingRegistrationApplicationService dataProcessingRegistrationApplicationService,
            IOrganizationRightsService organizationRightsService, 
            IRoleAssignmentService<ItContractRight, ItContractRole, ItContract> itContractRightService, 
            IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage> itSystemRightService, 
            IRoleAssignmentService<ItProjectRight, ItProjectRole, ItProject> itProjectRightService,
            IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit> organizationUnitRightService)
        {
            _dataProcessingRegistrationApplicationService = dataProcessingRegistrationApplicationService;
            _organizationRightsService = organizationRightsService;
            _itContractRightService = itContractRightService;
            _itSystemRightService = itSystemRightService;
            _itProjectRightService = itProjectRightService;
            _organizationUnitRightService = organizationUnitRightService;
        }

        public void Handle(EntityBeingDeletedEvent<User> domainEvent)
        {
            var user = domainEvent.Entity;

            ClearDataProcessingRegistrationRight(user);
            ClearOrganizationRights(user);
            ClearItContractRights(user);
            ClearItSystemRights(user);
            ClearItProjectRights(user);
            ClearOrganizationUnitRights(user);
        }

        private void ClearDataProcessingRegistrationRight(User user)
        {
            var roles = user.DataProcessingRegistrationRights;
            if (roles == null)
                return;

            roles.ToList().ForEach(x => _dataProcessingRegistrationApplicationService.RemoveRole(x.Id, x.RoleId, user.Id).ThrowOnFailure());
            roles.Clear();
        }

        private void ClearOrganizationRights(User user)
        {
            var roles = user.OrganizationRights;
            if (roles == null)
                return;

            roles.ToList().ForEach(x => _organizationRightsService.RemoveRole(x.OrganizationId, user.Id, x.Role).ThrowOnFailure());
            roles.Clear();
        }

        private void ClearItContractRights(User user)
        {
            var roles = user.ItContractRights;
            if (roles == null)
                return;

            roles.ToList().ForEach(x => _itContractRightService.RemoveRole(x.Object, x.RoleId, user.Id).ThrowOnFailure());
            roles.Clear();
        }

        private void ClearItSystemRights(User user)
        {
            var roles = user.ItSystemRights;
            if (roles == null)
                return;

            roles.ToList().ForEach(x => _itSystemRightService.RemoveRole(x.Object, x.RoleId, user.Id).ThrowOnFailure());
            roles.Clear();
        }

        private void ClearItProjectRights(User user)
        {
            var roles = user.ItProjectRights;
            if (roles == null)
                return;

            roles.ToList().ForEach(x => _itProjectRightService.RemoveRole(x.Object, x.RoleId, user.Id).ThrowOnFailure());
            roles.Clear();
        }

        private void ClearOrganizationUnitRights(User user)
        {
            var roles = user.OrganizationUnitRights;
            if (roles == null)
                return;

            roles.ToList().ForEach(x => _organizationUnitRightService.RemoveRole(x.Object, x.RoleId, user.Id).ThrowOnFailure());
            roles.Clear();
        }

    }
}
