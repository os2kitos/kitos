using Core.DomainModel.PublicMessage;

namespace Infrastructure.DataAccess.Mapping
{
    public class PublicMessageMap : EntityMap<PublicMessage>
    {
        public PublicMessageMap()
        {
            // Properties
            // Table & Column Mappings
            ToTable("PublicMessages");

            Property(x => x.ShortDescription).HasMaxLength(PublicMessage.DefaultShortDescriptionMaxLength);
            Property(x => x.Title).HasMaxLength(PublicMessage.DefaultTitleMaxLength);

            Property(x => x.Uuid)
                .IsRequired()
                .HasUniqueIndexAnnotation("UX_PublicMessage_Uuid", 0);
        }
    }
}
