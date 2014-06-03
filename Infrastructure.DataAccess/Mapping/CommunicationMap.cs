using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class CommunicationMap : EntityMap<Communication>
    {
        public CommunicationMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("Communication");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ItProjectId).HasColumnName("ItProjectId");

            // Relationships
            this.HasRequired(t => t.ItProject)
                .WithMany(t => t.Communications)
                .HasForeignKey(d => d.ItProjectId);

            this.HasOptional(t => t.ResponsibleUser)
                .WithMany(t => t.ResponsibleForCommunications)
                .HasForeignKey(d => d.ResponsibleUserId);
        }
    }
}
