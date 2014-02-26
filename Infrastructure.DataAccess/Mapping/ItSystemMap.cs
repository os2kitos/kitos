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
                .HasForeignKey(d => d.ParentItSystem_Id);

            this.HasRequired(t => t.Municipality)
                .WithMany(t => t.ItSystems)
                .HasForeignKey(d => d.Municipality_Id)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.SystemType)
                .WithMany(t => t.ItSystem)
                .HasForeignKey(d => d.SystemType_Id)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.InterfaceType)
                .WithMany(t => t.ItSystems)
                .HasForeignKey(d => d.InterfaceType_Id)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.ProtocolType)
                .WithMany(t => t.ItSystems)
                .HasForeignKey(d => d.ProtocolType_Id)
                .WillCascadeOnDelete(false);
        }
    }
}
