using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class BasicDataMap : EntityTypeConfiguration<BasicData>
    {
        public BasicDataMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BasicData");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ItSystemId).HasColumnName("ItSystemId");

            // Relationships
            this.HasRequired(t => t.ItSystem)
                .WithMany(t => t.BasicDatas)
                .HasForeignKey(d => d.ItSystemId);

        }
    }
}
