using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.Text;

namespace Infrastructure.DataAccess.Mapping
{
    public class KitosIntroMap : EntityTypeConfiguration<KitosIntro>
    {
        public KitosIntroMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("KitosIntro");
            this.Property(t => t.Id).HasColumnName("Id");
        }
    }
}