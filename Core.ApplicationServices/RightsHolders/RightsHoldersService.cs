using System;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Interface;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries.Interface;
using Infrastructure.Services.Types;

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

        public Result<ItInterface, OperationError> GetInterfaceAsRightsHolder(Guid interfaceUuid)
        {
            return WithAnyRightsHoldersAccess()
                .Match
                (
                    error => error,
                    () => _itInterfaceService.GetInterface(interfaceUuid).Bind(WithRightsHolderAccessTo)
                );
        }



        public Result<IQueryable<ItInterface>, OperationError> GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess(Guid? rightsHolderUuid = null)
        {
            return WithAnyRightsHoldersAccess()
                .Match
                (
                    error => error,
                    () =>
                    {
                        var organizationIdsWhereUserHasRightsHoldersAccess = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess);
                        var query = new QueryByRightsHolderIdsOrOwnOrganizationIds(organizationIdsWhereUserHasRightsHoldersAccess);
                        return Result<IQueryable<ItInterface>, OperationError>.Success(_itInterfaceService.GetAvailableInterfaces(query));
                    }
                );
        }

        public IQueryable<Organization> ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess()
        {
            var organizationIds = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess).ToList();
            return _organizationRepository.AsQueryable().ByIds(organizationIds);
        }

        private Maybe<OperationError> WithAnyRightsHoldersAccess()
        {
            return _userContext.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess)
                ? Maybe<OperationError>.None
                : new OperationError("User does not have 'rightsholders access' in any organization", OperationFailure.Forbidden);
        }

        private Result<T, OperationError> WithRightsHolderAccessTo<T>(T subject) where T : IHasRightsHolder
        {
            //User may have read access in a different context (own systems but not with rightsholder set to a rightsholding organization) but in this case we insist that rightsholder access must be issued
            var hasAssignedRightsHolderAccess = subject
                .GetRightsHolderOrganizationId()
                .Select(organizationId => _userContext.HasRole(organizationId, OrganizationRole.RightsHolderAccess))
                .GetValueOrFallback(false);

            if (hasAssignedRightsHolderAccess)
                return subject;

            return new OperationError("Not rightsholder for the requested system", OperationFailure.Forbidden);
        }
    }
}
