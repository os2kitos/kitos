using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class MunicipalityMap : EntityTypeConfiguration<Municipality>
    {
        public MunicipalityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Municipality");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Setup_Id).HasColumnName("Setup_Id");

            // Relationships
            this.HasRequired(t => t.Configuration)
                .WithMany(t => t.MunicipalitySets)
                .HasForeignKey(d => d.Setup_Id);

        }
    }
}
