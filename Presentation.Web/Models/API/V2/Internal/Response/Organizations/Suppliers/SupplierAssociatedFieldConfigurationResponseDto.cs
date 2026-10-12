using Presentation.Web.Models.API.V2.Response.Supplier;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations.Suppliers;

public class SupplierAssociatedFieldConfigurationResponseDTO
{
    public required string FieldKey { get; set; }
    public FieldControlStateChoice ControlState { get; set; }
}