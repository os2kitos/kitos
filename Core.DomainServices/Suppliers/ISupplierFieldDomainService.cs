using Core.Abstractions.Types;
using Core.DomainModel.SupplierAssociatedFields;
using System;
using System.Collections.Generic;

namespace Core.DomainServices.Suppliers
{
    public interface ISupplierFieldDomainService
    {
        bool ContainsOnlySupplierControlledAndSharedFields(IEnumerable<string> properties, Guid organizationUuid);
        bool ContainsAnySupplierControlledFields(IEnumerable<string> properties, Guid organizationUuid);
        bool IsSupplierControlled(string key, Guid organizationUuid);
        Maybe<IEnumerable<SupplierAssociatedFieldConfiguration>> GetSupplierAssociatedFieldConfigurations(Guid organizationUuid);
    }
}
