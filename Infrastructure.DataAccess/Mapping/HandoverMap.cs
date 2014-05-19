using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class HandoverMap : EntityTypeConfiguration<Handover>
    {
        public HandoverMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Handover");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.ItProject)
                .WithOptional(t => t.Handover);

            this.HasMany(t => t.Participants)
                .WithMany(t => t.HandoverParticipants);
        }
    }
}
