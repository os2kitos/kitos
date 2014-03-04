using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class TechnologyMap : EntityTypeConfiguration<Technology>
    {
        public TechnologyMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Technology");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.DatabaseType_Id).HasColumnName("DatabaseType_Id");
            this.Property(t => t.Environment_Id).HasColumnName("Environment_Id");
            this.Property(t => t.ProgLanguage_Id).HasColumnName("ProgLanguage_Id");

            // Relationships
            this.HasRequired(t => t.DatabaseType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.DatabaseType_Id);
            this.HasRequired(t => t.Environment)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.Environment_Id);
            this.HasRequired(t => t.ItSystem)
                .WithOptional(t => t.Technology);
            this.HasRequired(t => t.ProgLanguage)
                .WithMany(t => t.Technologies)
                .HasForeignKey(d => d.ProgLanguage_Id);

        }
    }
}
