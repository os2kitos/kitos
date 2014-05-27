using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class AdviceMap : EntityTypeConfiguration<Advice>
    {
        public AdviceMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);
            
            // Table & Column Mappings
            this.ToTable("Advice");

            // Relationships
            this.HasRequired(t => t.ItContract)
                .WithMany(t => t.Advices)
                .HasForeignKey(d => d.ItContractId);
        }
    }
}
