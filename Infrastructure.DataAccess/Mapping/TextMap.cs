using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class TextMap : EntityMap<Text>
    {
        public TextMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("Text");
            this.Property(t => t.Value).HasColumnName("Value");

        }
    }
}