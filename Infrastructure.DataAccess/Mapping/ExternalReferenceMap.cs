using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class ExternalReferenceMap : EntityTypeConfiguration<ExternalReference>
    {
        public ExternalReferenceMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Value)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("ExternalReference");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Value).HasColumnName("Value");
            this.Property(t => t.ItProject_Id).HasColumnName("ItProject_Id");
            this.Property(t => t.ExternalReferenceType_Id).HasColumnName("ExternalReferenceType_Id");
            this.Property(t => t.ItSystem_Id).HasColumnName("ItSystem_Id");

            // Relationships
            this.HasRequired(t => t.ExternalReferenceType)
                .WithMany(t => t.ExternalReferences)
                .HasForeignKey(d => d.ExternalReferenceType_Id);
            this.HasRequired(t => t.ItProject)
                .WithMany(t => t.ExternalReferences)
                .HasForeignKey(d => d.ItProject_Id);
            this.HasRequired(t => t.ItSystem)
                .WithMany(t => t.ExternalReferences)
                .HasForeignKey(d => d.ItSystem_Id);

        }
    }
}
