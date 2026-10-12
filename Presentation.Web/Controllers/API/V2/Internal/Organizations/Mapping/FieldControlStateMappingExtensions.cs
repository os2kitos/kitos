using Core.Abstractions.Types;
using Core.DomainModel.SupplierAssociatedFields;
using Presentation.Web.Models.API.V2.Response.Supplier;

namespace Presentation.Web.Controllers.API.V2.Internal.Organizations.Mapping;

public static class FieldControlStateMappingExtensions
{
    private static readonly EnumMap<FieldControlStateChoice, FieldControlState> Mapping;

    static FieldControlStateMappingExtensions()
    {
        Mapping = new EnumMap<FieldControlStateChoice, FieldControlState>
        (
            (FieldControlStateChoice.Organization, FieldControlState.Organization),
            (FieldControlStateChoice.Supplier, FieldControlState.Supplier),
            (FieldControlStateChoice.Shared, FieldControlState.Shared)
        );
    }

    public static FieldControlState ToDomain(this FieldControlStateChoice value)
    {
        return Mapping.FromLeftToRight(value);
    }

    public static FieldControlStateChoice ToDto(this FieldControlState value)
    {
        return Mapping.FromRightToLeft(value);
    }
}
