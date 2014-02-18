using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.Text;

namespace Infrastructure.DataAccess.Mapping
{
    public class SecuritySchemeMap : EntityTypeConfiguration<SecurityScheme>
    {
        public SecuritySchemeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ItPreAnalysis");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Text).HasColumnName("Text");
        } 
    }
}