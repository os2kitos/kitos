using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class TextMap : EntityMap<Text>
    {
        public TextMap()
        {
            // Properties
            // Table & Column Mappings
            ToTable("Text");
            Property(t => t.Value).HasColumnName("Value");

        }
    }
}
