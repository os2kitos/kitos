using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.DomainModel.Organization;
using Core.DomainModel.SupplierAssociatedFields;

namespace Core.ApplicationServices.Organizations.Write
{
    public interface IOrganizationSupplierService
    {
        Result<IEnumerable<OrganizationSupplier>, OperationError> GetSuppliersForOrganization(Guid organizationUuid);
        Result<IEnumerable<Organization>, OperationError> GetAvailableSuppliers(Guid organizationUuid);
        Result<IEnumerable<Organization>, OperationError> GetUsingOrganizations(Guid supplierUuid);
        Result<OrganizationSupplier, OperationError> AddSupplierToOrganization(Guid organizationUuid, Guid supplierUuid);
        Maybe<OperationError> RemoveSupplierFromOrganization(Guid organizationUuid, Guid supplierUuid);

        Result<ISet<SupplierAssociatedFieldConfiguration>, OperationError> GetSupplierFieldConfigurations(Guid organizationUuid);
        Result<ISet<SupplierAssociatedFieldConfiguration>, OperationError> UpsertSupplierFieldConfigurations(
            Guid organizationUuid,
            SupplierAssociatedFieldConfigurationUpdateParameters parameters);
    }
}
