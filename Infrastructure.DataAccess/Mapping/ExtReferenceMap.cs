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
            this.Property(t => t.ItProjectId).HasColumnName("ItProjectId");
            this.Property(t => t.ExtReferenceTypeId).HasColumnName("ExtReferenceTypeId");
            this.Property(t => t.ItSystemId).HasColumnName("ItSystemId");

            // Relationships
            this.HasRequired(t => t.ExtReferenceType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ExtReferenceTypeId);
            this.HasRequired(t => t.ItProject)
                .WithMany(t => t.ExtReferences)
                .HasForeignKey(d => d.ItProjectId);
            this.HasRequired(t => t.ItSystem)
                .WithMany(t => t.ExtReferences)
                .HasForeignKey(d => d.ItSystemId);

        }
    }
}
