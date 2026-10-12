using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model;
using Core.DomainModel;

namespace Core.ApplicationServices.Authorization
{
    public interface ISupplierAssociatedFieldsService
    {
        bool HasAnySupplierChanges(ISupplierAssociatedEntityUpdateParameters parameters, IEntity entity, Guid organizationUuid);
        bool HasOnlySupplierChanges(ISupplierAssociatedEntityUpdateParameters parameters, IEntity entity, Guid organizationUuid);
        bool HasAnySupplierChangesList(IEnumerable<ISupplierAssociatedEntityUpdateParameters> parametersEnumerable, IEntity entity, Guid organizationUuid);
        bool RequestsDeleteToEntity<TEntity>(TEntity entity);
        bool IsFieldSupplierControlled(string key, Guid organizationUuid);
    }
}
