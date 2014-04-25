using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataRowMap : EntityTypeConfiguration<DataRow>
    {
        public DataRowMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("DataRow");
            this.Property(t => t.Id).HasColumnName("Id");

            this.HasRequired(t => t.DataType)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.DataTypeId);

            this.HasRequired(t => t.ItSystem)
                .WithMany(d => d.DataRows)
                .HasForeignKey(t => t.ItSystemId)
                .WillCascadeOnDelete(true);

        }
    }
}