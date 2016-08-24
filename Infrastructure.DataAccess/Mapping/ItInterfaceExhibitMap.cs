using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    class ItInterfaceExhibitMap : EntityMap<ItInterfaceExhibit>
    {
        public ItInterfaceExhibitMap()
        {
            // Table & Column Mappings
            this.ToTable("Exhibit"); // TODO bad name, it's not intuitive when looking in the databas

           // Relationships
            this.HasRequired(t => t.ItInterface)
                .WithOptional(t => t.ExhibitedBy)
                .WillCascadeOnDelete(true);
        }
    }
}
