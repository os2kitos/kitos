using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataRowUsageMap : EntityTypeConfiguration<DataRowUsage>
    {
        public DataRowUsageMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("DataRowUsage");
            this.Property(t => t.Id).HasColumnName("Id");

            this.HasRequired(t => t.InterfaceUsage)
                .WithMany(d => d.DataRowUsages)
                .HasForeignKey(t => t.InterfaceUsageId)
                .WillCascadeOnDelete(true);

            this.HasRequired(t => t.DataRow)
                .WithMany(d => d.Usages)
                .HasForeignKey(t => t.DataRowId)
                .WillCascadeOnDelete(true);

            this.HasOptional(t => t.Frequency)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.FrequencyId);

        }
    }
}