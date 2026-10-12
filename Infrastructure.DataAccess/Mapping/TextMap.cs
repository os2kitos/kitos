using Core.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class TextMap : EntityMap<Text>
    {
        public override void Configure(EntityTypeBuilder<Text> builder)
        {
            base.Configure(builder);
            builder.ToTable("Text");
            builder.Property(t => t.Value).HasColumnName("Value");
        }
    }
}
