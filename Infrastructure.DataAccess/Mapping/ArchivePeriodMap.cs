using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ArchivePeriodMap : EntityMap<ArchivePeriod>
    {
        public ArchivePeriodMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("ArchivePeriod");

            this.HasRequired(t => t.ItSystemUsage)
                .WithMany(t => t.ArchivePeriods)
                .HasForeignKey(d => d.ItSystemUsageId);

            Property(x => x.Uuid)
                .IsRequired()
                .HasIndexAnnotation("UX_ArchivePeriod_Uuid");
        }
    }
}
