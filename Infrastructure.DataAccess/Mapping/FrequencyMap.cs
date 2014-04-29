using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class FrequencyMap : EntityTypeConfiguration<Frequency>
    {
        public FrequencyMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Frequency");
            this.Property(t => t.Id).HasColumnName("Id");

        }
    }
}