using System.Collections.Generic;
using Core.DomainModel.SupplierAssociatedFields;

namespace Core.ApplicationServices.Model.Organizations.Write
{
    public class SupplierAssociatedFieldConfigurationUpdateParameters
    {
        public required IEnumerable<SupplierAssociatedFieldConfiguration> Configurations { get; set; }
    }
}
