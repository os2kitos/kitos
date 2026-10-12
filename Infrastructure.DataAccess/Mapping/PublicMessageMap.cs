using Core.DomainModel.PublicMessage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class PublicMessageMap : EntityMap<PublicMessage>
    {
        public override void Configure(EntityTypeBuilder<PublicMessage> builder)
        {
            base.Configure(builder);
            base.Configure(builder);
            builder.ToTable("PublicMessages");

            builder.Property(x => x.ShortDescription).HasMaxLength(PublicMessage.DefaultShortDescriptionMaxLength);
            builder.Property(x => x.Title).HasMaxLength(PublicMessage.DefaultTitleMaxLength);

            builder.Property(x => x.Uuid).IsRequired();
            builder.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("UX_PublicMessage_Uuid");
        }
    }
}
