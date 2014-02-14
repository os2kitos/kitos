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
            this.Property(t => t.ItSystem_Id).HasColumnName("ItSystem_Id");

            // Relationships
            this.HasRequired(t => t.ItSystem)
                .WithMany(t => t.BasicDatas)
                .HasForeignKey(d => d.ItSystem_Id);

        }
    }
}
