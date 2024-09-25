using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices.Repositories.Organization;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.Organizations.Write;

public class OrganizationWriteService : IOrganizationWriteService{
        
    private readonly ITransactionManager _transactionManager;               
    private readonly IDomainEvents _domainEvents;
    private readonly IOrganizationService _organizationService;
    private readonly IAuthorizationContext _authorizationContext;
    private readonly IOrganizationRepository _repository;

    public OrganizationWriteService(ITransactionManager transactionManager, IDomainEvents domainEvents, IOrganizationService organizationService, IAuthorizationContext authorizationContext, IOrganizationRepository repository)
    {
        _transactionManager = transactionManager;
        _domainEvents = domainEvents;
        _organizationService = organizationService;
        _authorizationContext = authorizationContext;
        _repository = repository;
    }

    public Result<Organization, OperationError> UpdateMasterData(Guid organizationUuid, OrganizationMasterDataUpdateParameters parameters)
    {
        using var transaction = _transactionManager.Begin();
        var result = _organizationService.GetOrganization(organizationUuid)
            .Bind(organization => Update(organization, parameters));

        if (result.Ok) transaction.Commit();
        else transaction.Rollback();

        return result;
    }

    private Result<Organization, OperationError> Update(Organization organization, OrganizationMasterDataUpdateParameters parameters)
    {
        using var transaction = _transactionManager.Begin();

        var result = WithWriteAccess(organization)
            .Bind(organization => PerformUpdates(organization, parameters));

        if (result.Ok)
        {
            _domainEvents.Raise(new EntityUpdatedEvent<Organization>(result.Value));
            _repository.Update(result.Value);
            transaction.Commit();
        }
        else transaction.Rollback();

        return result;
    }

    private Result<Organization, OperationError> WithWriteAccess(Organization org)
    {
        return _authorizationContext.AllowModify(org) ? org : new OperationError(OperationFailure.Forbidden);
    }

    private Result<Organization, OperationError> PerformUpdates(Organization organization, OrganizationMasterDataUpdateParameters parameters)
    {
        return organization
            .WithOptionalUpdate(parameters.Address, UpdateOrganizationAddress)
            .Bind(org => org.WithOptionalUpdate(parameters.Cvr, UpdateOrganizationCvr))
            .Bind(org => org.WithOptionalUpdate(parameters.Email, UpdateOrganizationEmail))
            .Bind(org => org.WithOptionalUpdate(parameters.Phone, UpdateOrganizationPhone));
    }

    private Result<Organization, OperationError> UpdateOrganizationAddress(Organization organization,
        string address)
    {
        organization.Adress = address;
        return organization;
    }
    private Result<Organization, OperationError> UpdateOrganizationCvr(Organization organization,
        string cvr)
    {
        organization.Cvr = cvr;
        return organization;
    }
    private Result<Organization, OperationError> UpdateOrganizationEmail(Organization organization,
        string email)
    {
        organization.Email = email;
        return organization;
    }
    private Result<Organization, OperationError> UpdateOrganizationPhone(Organization organization,
        string phone)
    {
        organization.Phone = phone;
        return organization;
    }
}