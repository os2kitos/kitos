using Core.ApplicationServices.Model;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Mapping.Authorization;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.DomainServices.Suppliers;
using System;

namespace Core.ApplicationServices.Authorization;

public class SupplierAssociatedFieldsService : ISupplierAssociatedFieldsService
{
    private readonly ISupplierFieldDomainService _supplierFieldDomainService;
    private readonly ISupplierAssociatedFieldKeyMapper _mapper;

    public SupplierAssociatedFieldsService(ISupplierFieldDomainService supplierFieldDomainService, ISupplierAssociatedFieldKeyMapper mapper)
    {
        _supplierFieldDomainService = supplierFieldDomainService;
        _mapper = mapper;
    }

    public bool HasAnySupplierChanges(ISupplierAssociatedEntityUpdateParameters parameters, IEntity entity, Guid organizationUuid)
    {
        return parameters switch
        {
            DataProcessingRegistrationModificationParameters dprParameters => HasDprSupplierChanges(dprParameters, entity, organizationUuid),
            UpdatedDataProcessingRegistrationOversightDateParameters oversightDateParameters =>
                HasOversightDateSupplierChanges(oversightDateParameters, entity, organizationUuid),
            SystemUsageUpdateParameters usageParameters => HasUsageSupplierChanges(usageParameters, entity, organizationUuid),
            _ => false
        };
    }

    public bool HasOnlySupplierChanges(ISupplierAssociatedEntityUpdateParameters parameters, IEntity entity, Guid organizationUuid)
    {
        return parameters switch
        {
            DataProcessingRegistrationModificationParameters dprParameters => HasOnlyDprSupplierChanges(
                dprParameters, entity, organizationUuid),
            UpdatedDataProcessingRegistrationOversightDateParameters oversightDateParameters =>
                HasOnlyOversightDateSupplierChanges(oversightDateParameters, entity, organizationUuid),
            SystemUsageUpdateParameters usageParameters => HasOnlyUsageSupplierChanges(usageParameters, entity, organizationUuid),
            _ => false
        };
    }

    public bool RequestsDeleteToEntity<TEntity>(TEntity entity)
    {
        return entity switch
        {
            DataProcessingRegistrationOversightDate => true,
            _ => false
        };
    }

    public bool HasAnySupplierChangesList(IEnumerable<ISupplierAssociatedEntityUpdateParameters> parametersEnumerable, IEntity entity, Guid organizationUuid)
    {
        var results = parametersEnumerable.Select(x => HasAnySupplierChanges(x, entity, organizationUuid));
        return results.Any(r => r);
    }

    private bool HasDprSupplierChanges(DataProcessingRegistrationModificationParameters dprParams, IEntity entity, Guid organizationUuid)
    {
        if (entity is not DataProcessingRegistration dpr)
            return false;
        
        var changedProperties = dprParams.GetChangedPropertyKeys(dpr);
        
        return _supplierFieldDomainService.ContainsAnySupplierControlledFields(_mapper.MapParameterKeysToDomainKeys(changedProperties, entity), organizationUuid);
    }

    private bool HasOversightDateSupplierChanges(UpdatedDataProcessingRegistrationOversightDateParameters parameters, IEntity entity, Guid organizationUuid)
    {
        var changedProperties = parameters.GetChangedPropertyKeys();
        var keys = _mapper.MapParameterKeysToDomainKeys(changedProperties, entity);
        return _supplierFieldDomainService.ContainsAnySupplierControlledFields(keys, organizationUuid);
    }

    private bool HasUsageSupplierChanges(SystemUsageUpdateParameters parameters, IEntity entity, Guid organizationUuid)
    {
        var changedProperties = parameters.GetChangedPropertyKeys();
        var keys = _mapper.MapParameterKeysToDomainKeys(changedProperties, entity);

        return _supplierFieldDomainService.ContainsAnySupplierControlledFields(keys, organizationUuid);
    }

    public bool IsFieldSupplierControlled(string key, Guid organizationUuid)
    {
        return _supplierFieldDomainService.IsSupplierControlled(key, organizationUuid);
    }

    private bool HasOnlyOversightDateSupplierChanges(UpdatedDataProcessingRegistrationOversightDateParameters parameters, IEntity entity, Guid organizationUuid)
    {
        var changedProperties = parameters.GetChangedPropertyKeys();
        return _supplierFieldDomainService.ContainsOnlySupplierControlledAndSharedFields(_mapper.MapParameterKeysToDomainKeys(changedProperties, entity), organizationUuid);
    }

    private bool HasOnlyDprSupplierChanges(DataProcessingRegistrationModificationParameters dprParams, IEntity entity, Guid organizationUuid)
    {
        if(entity is not DataProcessingRegistration dpr)
            return false;

        var changedProperties = dprParams.GetChangedPropertyKeys(dpr);

        return _supplierFieldDomainService.ContainsOnlySupplierControlledAndSharedFields(_mapper.MapParameterKeysToDomainKeys(changedProperties, entity), organizationUuid);
    }

    private bool HasOnlyUsageSupplierChanges(SystemUsageUpdateParameters parameters, IEntity entity, Guid organizationUuid)
    {
        var changedProperties = parameters.GetChangedPropertyKeys();
        var keys = _mapper.MapParameterKeysToDomainKeys(changedProperties, entity);
        return _supplierFieldDomainService.ContainsOnlySupplierControlledAndSharedFields(keys, organizationUuid);
    }
}

