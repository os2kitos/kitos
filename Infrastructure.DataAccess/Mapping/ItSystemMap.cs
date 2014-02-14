using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemMap : EntityTypeConfiguration<ItSystem>
    {
        public ItSystemMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ItSystem");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ParentItSystem_Id).HasColumnName("ParentItSystem_Id");
            this.Property(t => t.Municipality_Id).HasColumnName("Municipality_Id");

            // Relationships
            this.HasRequired(t => t.ParentItSystem)
                .WithMany(t => t.ItSystems1)
                .HasForeignKey(d => d.ParentItSystem_Id);
            this.HasRequired(t => t.Municipality)
                .WithMany(t => t.ItSystems)
                .HasForeignKey(d => d.Municipality_Id);

        }
    }
}
