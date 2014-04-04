using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class RiskMap : EntityTypeConfiguration<Risk>
    {
        public RiskMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Risk");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ItProjectId).HasColumnName("ItProjectId");

            // Relationships
            this.HasRequired(t => t.ItProject)
                .WithMany(t => t.Risks)
                .HasForeignKey(d => d.ItProjectId);

        }
    }
}
