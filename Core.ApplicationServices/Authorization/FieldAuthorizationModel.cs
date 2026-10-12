using Core.Abstractions.Types;
using Core.ApplicationServices.Model;
using Core.DomainModel;
using System;
using System.Linq;

namespace Core.ApplicationServices.Authorization;

public class FieldAuthorizationModel : IAuthorizationModel, IFieldAuthorizationModel
{
    private readonly IOrganizationalUserContext _activeUserContext;
    private readonly ISupplierAssociatedFieldsService _supplierAssociatedFieldsService;
    private readonly IAuthorizationContext _authorizationContext;

    public FieldAuthorizationModel(IOrganizationalUserContext activeUserContext, ISupplierAssociatedFieldsService supplierAssociatedFieldsService, IAuthorizationContext authorizationContext)
    {
        _activeUserContext = activeUserContext;
        _supplierAssociatedFieldsService = supplierAssociatedFieldsService;
        _authorizationContext = authorizationContext;
    }

    public Result<bool, OperationError> AuthorizeUpdate(
        IEntityOwnedByOrganization? entity,
        ISupplierAssociatedEntityUpdateParameters? parameters)
    {
        if (entity == null || parameters == null) return false;

        if (_activeUserContext.IsGlobalAdmin()) return true;
        var entityOrganization = entity.Organization;
        var organizationHasSuppliers = entityOrganization?.HasSuppliers() ?? false;
        if (!organizationHasSuppliers) return _authorizationContext.AllowModify(entity);

        var supplierIds = entityOrganization!.Suppliers.ToHashSet().Select(x => x.SupplierId);
        var userHasSupplierApiAccess = _activeUserContext.IsSupplierApiUserForOrganizationWithSuppliers(supplierIds);
         return userHasSupplierApiAccess
            ? CheckForSupplierApiUser(entity, parameters)
            : CheckForNonSupplierApiUser(entity, parameters);
    }

    public bool AuthorizeChildEntityDelete<TChild>(IEntityOwnedByOrganization? parent, TChild? child) where TChild : class
    {
        if (parent == null || child == null) return false;

        if (_activeUserContext.IsGlobalAdmin()) return true;
        var entityOrganization = parent.Organization;
        var organizationHasSuppliers = entityOrganization?.HasSuppliers() ?? false;
        if (!organizationHasSuppliers) return _authorizationContext.AllowModify(parent);

        var supplierIds = entityOrganization!.Suppliers.ToHashSet().Select(x => x.SupplierId);
        var userHasSupplierApiAccess = _activeUserContext.IsSupplierApiUserForOrganizationWithSuppliers(supplierIds);
        var requestsDeleteForSupplierControlledEntity = _supplierAssociatedFieldsService.RequestsDeleteToEntity(child);
        return userHasSupplierApiAccess
            ? requestsDeleteForSupplierControlledEntity
            : requestsDeleteForSupplierControlledEntity == false;
    }

    private Result<bool, OperationError> CheckForSupplierApiUser(IEntityOwnedByOrganization entity,
        ISupplierAssociatedEntityUpdateParameters parameters)
    {
        return WithOrganizationUuid(entity)
            .Select(
                (organizationUuid) =>
                    {
                        var hasOnlySupplierChanges = _supplierAssociatedFieldsService.HasOnlySupplierChanges(parameters, entity, organizationUuid);
                        return hasOnlySupplierChanges || _authorizationContext.AllowModify(entity);
                    }
            );
    }

    private Result<bool, OperationError> CheckForNonSupplierApiUser(IEntityOwnedByOrganization entity,
        ISupplierAssociatedEntityUpdateParameters parameters)
    {
        return WithOrganizationUuid(entity)
            .Select(
                (organizationUuid) =>
                    {
                        var anySupplierChanges = _supplierAssociatedFieldsService.HasAnySupplierChanges(parameters, entity, organizationUuid);
                        return !anySupplierChanges && _authorizationContext.AllowModify(entity);
                    }
            );       
    }

    private Result<Guid, OperationError> WithOrganizationUuid(IEntityOwnedByOrganization entity)
    {
        var organizationUuid = entity.Organization?.Uuid;
        if (!organizationUuid.HasValue) return new OperationError($"No organization UUID found for {typeof(IEntityOwnedByOrganization)} with Id {entity.Id}", OperationFailure.BadState);
        return organizationUuid.Value;
    }

    public FieldPermissionsResult GetFieldPermissions(IEntityOwnedByOrganization entity, string key, Guid organizationUuid)
    {
        if (_activeUserContext.IsGlobalAdmin()) 
            return new FieldPermissionsResult{ Enabled = true, Key = key};

        var entityOrganization = entity.Organization;
        if (!_authorizationContext.AllowModify(entity))
            return new FieldPermissionsResult { Enabled = false, Key = key };

        var organizationHasSuppliers = entityOrganization?.HasSuppliers() ?? false;
        if (!organizationHasSuppliers)
            return new FieldPermissionsResult { Enabled = true, Key = key };

        return new FieldPermissionsResult
                { Enabled = _supplierAssociatedFieldsService.IsFieldSupplierControlled(key, organizationUuid) == false, Key = key };
    }
}
