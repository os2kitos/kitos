using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class KLEMap : EntityTypeConfiguration<KLE>
    {
        public KLEMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("KLE");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ItProject_Id).HasColumnName("ItProject_Id");
            this.Property(t => t.ItSystem_Id).HasColumnName("ItSystem_Id");

            // Relationships
            this.HasRequired(t => t.ItProject)
                .WithMany(t => t.KLEs)
                .HasForeignKey(d => d.ItProject_Id);
            this.HasRequired(t => t.ItSystem)
                .WithMany(t => t.KLEs)
                .HasForeignKey(d => d.ItSystem_Id);

        }
    }
}
