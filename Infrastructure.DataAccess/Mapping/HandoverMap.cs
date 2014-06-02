using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class HandoverMap : EntityMap<Handover>
    {
        public HandoverMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("Handover");

            // Relationships
            this.HasRequired(t => t.ItProject)
                .WithOptional(t => t.Handover)
                .WillCascadeOnDelete(true);

            this.HasMany(t => t.Participants)
                .WithMany(t => t.HandoverParticipants);
        }
    }
}
