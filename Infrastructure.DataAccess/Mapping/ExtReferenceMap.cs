using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class ExtReferenceMap : EntityTypeConfiguration<ExtReference>
    {
        public ExtReferenceMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Value)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("ExtReference");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Value).HasColumnName("Value");
            this.Property(t => t.ItProject_Id).HasColumnName("ItProject_Id");
            this.Property(t => t.ExtReferenceType_Id).HasColumnName("ExtReferenceType_Id");
            this.Property(t => t.ItSystem_Id).HasColumnName("ItSystem_Id");

            // Relationships
            this.HasRequired(t => t.ExtReferenceType)
                .WithMany(t => t.ExtReferences)
                .HasForeignKey(d => d.ExtReferenceType_Id);
            this.HasRequired(t => t.ItProject)
                .WithMany(t => t.ExtReferences)
                .HasForeignKey(d => d.ItProject_Id);
            this.HasRequired(t => t.ItSystem)
                .WithMany(t => t.ExtReferences)
                .HasForeignKey(d => d.ItSystem_Id);

        }
    }
}
