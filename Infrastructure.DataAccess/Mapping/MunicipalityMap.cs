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

            // Relationships
            this.HasMany(t => t.Configurations)
                .WithRequired(t => t.Municipality)
                .HasForeignKey(d => d.Municipality_Id)
                .WillCascadeOnDelete(true);

            this.HasMany(t => t.Localizations)
                .WithRequired(t => t.Municipality)
                .HasForeignKey(d => d.Municipality_Id)
                .WillCascadeOnDelete(true);
        }
    }
}
