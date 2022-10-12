using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Core.DomainServices.Role;
using dk.nita.saml20.Schema.Metadata;

namespace Core.ApplicationServices.Organizations
{
    public class OrganizationRegistrationService : IOrganizationRegistrationService
    {
        private readonly IEntityIdentityResolver _identityResolver;
        private readonly IOrganizationService _organizationService;
        private readonly IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit> _organizationUnitRightService;
        public OrganizationRegistrationService(IEntityIdentityResolver identityResolver, 
            IOrganizationService organizationService,
            IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit> organizationUnitRightService)
        {
            _organizationUnitRightService = organizationUnitRightService;
            _organizationService = organizationService;
            _identityResolver = identityResolver;
        }

        public Result<OrganizationRegistrationsRoot, OperationError> GetOrganizationRegistrations(int unitId)
        {
            throw new NotImplementedException();
            //var registrationsRoot = new OrganizationRegistrationsRoot();
            //var organizationUnitRights = GetOrganizationUnitRights(unitId);
        }

        public Maybe<OperationError> DeleteSelectedOrganizationRegistrations()
        {
            throw new NotImplementedException();
        }

        public Maybe<OperationError> TransferSelectedOrganizationRegistrations()
        {
            throw new NotImplementedException();
        }

       /* private Result<List<OrganizationRegistrationDetails>, OperationError> GetOrganizationUnitRights(int unitId)
        {
            var roles = GetApplicableUnitRoles(unitId);
            if (roles.Failed)
                return roles.Error;

            return roles.Value.Select()
        }

        private Result<IEnumerable<OrganizationUnitRole>, OperationError> GetApplicableUnitRoles(int unitId)
        {
            var unitUuid = _identityResolver.ResolveUuid<OrganizationUnit>(unitId);
            if (unitUuid.IsNone)
            {
                return new OperationError("Organization id is invalid", OperationFailure.BadInput);
            }

            var unit = _organizationService.GetOrganizationUnit(unitUuid.Value);
            if (unit.Failed)
            {
                return unit.Error;
            }

            return _organizationUnitRightService.GetApplicableRoles(unit.Value).ToList();
        }

        private OrganizationRegistrationDetails ToDetailsFromUnitRights(OrganizationUnitRole roles)
        {
            return new OrganizationRegistrationDetails()
            {

            }
        }*/
    }
}
