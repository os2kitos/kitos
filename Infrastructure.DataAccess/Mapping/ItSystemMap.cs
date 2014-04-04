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

            // Relationships
            this.HasRequired(t => t.ParentItSystem)
                .WithMany(t => t.ItSystems1)
                .HasForeignKey(d => d.ParentItSystemId);

            this.HasRequired(t => t.Organization)
                .WithMany(t => t.ItSystems)
                .HasForeignKey(d => d.MunicipalityId)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.SystemType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.SystemTypeId)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.InterfaceType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.InterfaceTypeId)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.ProtocolType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ProtocolTypeId)
                .WillCascadeOnDelete(false);
        }
    }
}
