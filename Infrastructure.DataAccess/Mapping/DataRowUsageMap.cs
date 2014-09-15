using Core.DomainModel.ItSystemUsage;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataRowUsageMap : EntityMap<DataRowUsage>
    {
        public DataRowUsageMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("DataRowUsage");

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