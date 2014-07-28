using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class HandoverTrialMap : EntityMap<HandoverTrial>
    {
        public HandoverTrialMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Table & Column Mappings
            this.ToTable("HandoverTrial");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.ItContract)
                .WithMany(t => t.HandoverTrials)
                .HasForeignKey(d => d.ItContractId);
        }
    }
}
