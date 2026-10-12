namespace Core.DomainModel.SupplierAssociatedFields
{
    public class SupplierAssociatedFieldConfiguration: Entity, IOwnedByOrganization
    {
        public string FieldKey { get; set; }
        public FieldControlState ControlState { get; set; }
        public int OrganizationId { get; set; }
        public virtual Organization.Organization Organization { get; set; }

        public bool HasSupplierControlState => ControlState == FieldControlState.Supplier;
        public bool HasSharedControlState => ControlState == FieldControlState.Shared;
    }
}
