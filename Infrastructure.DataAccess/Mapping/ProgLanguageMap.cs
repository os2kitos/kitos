using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ProgLanguageMap : EntityTypeConfiguration<ProgLanguage>
    {
        public ProgLanguageMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ProgLanguage");
            this.Property(t => t.Id).HasColumnName("Id");
        }
    }
}
