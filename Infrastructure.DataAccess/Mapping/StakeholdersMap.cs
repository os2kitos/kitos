using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class StakeholdersMap : EntityTypeConfiguration<Stakeholder>
    {
        public StakeholdersMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Stakeholder");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ItProjectId).HasColumnName("ItProjectId");

            // Relationships
            //this.HasRequired(t => t.ItProject)
            //    .WithMany(t => t.Stakeholders)
            //    .HasForeignKey(d => d.ItProjectId);

        }
    }
}
