
using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Interface;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Interface;

namespace Core.ApplicationServices.RightsHolders
{
    public class RightsHoldersService : IRightsHoldersService
    {
        private readonly IOrganizationalUserContext _userContext;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IItInterfaceService _itInterfaceService;

        public RightsHoldersService(
            IOrganizationalUserContext userContext, 
            IGenericRepository<Organization> organizationRepository, 
            IItInterfaceService itInterfaceService)
        {
            _userContext = userContext;
            _organizationRepository = organizationRepository;
            _itInterfaceService = itInterfaceService;
        }

        public Result<ItInterface, OperationError> GetInterfaceForRightsHolder(Guid interfaceUuid)
        {
            if (!_userContext.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess))
            {
                return new OperationError(OperationFailure.Forbidden);
            }
            return _itInterfaceService.GetInterface(interfaceUuid);
        }

        public Result<IQueryable<ItInterface>, OperationError> GetInterfacesForRightsHolder(Guid? rightsHolderUuid = null)
        {
            if(!_userContext.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess))
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            var refinements = new List<IDomainQuery<ItInterface>>();
            if (rightsHolderUuid.HasValue)
            {
                var org = _organizationRepository.AsQueryable().ByUuid(rightsHolderUuid.Value);
                if (!_userContext.HasRole(org.Id, OrganizationRole.RightsHolderAccess))
                {
                    return new OperationError(OperationFailure.Forbidden);
                }
                refinements.Add(new QueryByRightsHolder(rightsHolderUuid.Value));
            }


            return Result<IQueryable<ItInterface>, OperationError>.Success(_itInterfaceService.GetAvailableInterfaces(refinements.ToArray()));
        }

        public IQueryable<Organization> ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess()
        {
            var organizationIds = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess).ToList();
            return _organizationRepository.AsQueryable().ByIds(organizationIds);
        }
    }
}
