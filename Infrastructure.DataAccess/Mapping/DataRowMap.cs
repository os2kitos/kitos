using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataRowMap : EntityMap<DataRow>
    {
        public DataRowMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("DataRow");

            this.HasOptional(t => t.DataType)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.DataTypeId);

            this.HasRequired(t => t.ItInterface)
                .WithMany(d => d.DataRows)
                .HasForeignKey(t => t.ItInterfaceId)
                .WillCascadeOnDelete(false);
        }
    }
}
