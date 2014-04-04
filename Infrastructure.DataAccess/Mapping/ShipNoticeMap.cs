using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class ShipNoticeMap : EntityTypeConfiguration<ShipNotice>
    {
        public ShipNoticeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.AlarmDate)
                .IsRequired();

            this.Property(t => t.To)
                .IsRequired();

            this.Property(t => t.Cc)
                .IsRequired();

            this.Property(t => t.Subject)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("ShipNotice");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.AlarmDate).HasColumnName("AlarmDate");
            this.Property(t => t.To).HasColumnName("To");
            this.Property(t => t.Cc).HasColumnName("Cc");
            this.Property(t => t.Subject).HasColumnName("Subject");
            this.Property(t => t.ItContractId).HasColumnName("ItContractId");

            // Relationships
            this.HasRequired(t => t.ItContract)
                .WithMany(t => t.ShipNotices)
                .HasForeignKey(d => d.ItContractId);
        }
    }
}
