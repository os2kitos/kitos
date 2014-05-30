using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class RiskMap : EntityMap<Risk>
    {
        public RiskMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("Risk");
            this.Property(t => t.ItProjectId).HasColumnName("ItProjectId");

            this.HasOptional(t => t.ResponsibleUser)
                .WithMany(d => d.ResponsibleForRisks)
                .HasForeignKey(t => t.ResponsibleUserId)
                .WillCascadeOnDelete(false);

        }
    }
}
