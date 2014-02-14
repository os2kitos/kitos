using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class PreAnalysisMap : EntityTypeConfiguration<PreAnalysis>
    {
        public PreAnalysisMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PreAnalysis");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.ItProject)
                .WithOptional(t => t.PreAnalysis);

        }
    }
}
