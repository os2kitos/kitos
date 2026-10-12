using Presentation.Web.Models.API.V2.Response.Supplier;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Request.Supplier
{
    public class SupplierAssociatedFieldConfigurationRequestDTO
    {
        public required IEnumerable<SupplierAssociatedFieldConfigurationItemDTO> Configurations { get; set; }
    }

    public class SupplierAssociatedFieldConfigurationItemDTO
    {
        public required string FieldKey { get; set; }
        public FieldControlStateChoice ControlState { get; set; }
    }
}
