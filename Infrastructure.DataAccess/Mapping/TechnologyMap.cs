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
            this.Property(t => t.DatabaseTypeId).HasColumnName("DatabaseTypeId");
            this.Property(t => t.EnvironmentId).HasColumnName("EnvironmentId");
            this.Property(t => t.ProgLanguageId).HasColumnName("ProgLanguageId");

            // Relationships
            this.HasRequired(t => t.DatabaseType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.DatabaseTypeId);
            this.HasRequired(t => t.Environment)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.EnvironmentId);
            this.HasRequired(t => t.ItSystem)
                .WithOptional(t => t.Technology);
            this.HasRequired(t => t.ProgLanguage)
                .WithMany(t => t.Technologies)
                .HasForeignKey(d => d.ProgLanguageId);

        }
    }
}
