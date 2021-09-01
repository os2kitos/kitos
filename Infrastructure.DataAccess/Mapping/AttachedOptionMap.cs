using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class AttachedOptionMap : EntityMap<AttachedOption>
    {
        public AttachedOptionMap()
        {
            HasOptional(t => t.ObjectOwner)
                .WithMany()
                .HasForeignKey(d => d.ObjectOwnerId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.LastChangedByUser)
                .WithMany()
                .HasForeignKey(d => d.LastChangedByUserId)
                .WillCascadeOnDelete(false);

            Property(x => x.OptionType).HasIndexAnnotation("UX_OptionType", 0);
            Property(x => x.OptionId).HasIndexAnnotation("UX_OptionId", 1);
            Property(x => x.ObjectId).HasIndexAnnotation("UX_ObjectId", 2);
            Property(x => x.ObjectType).HasIndexAnnotation("UX_ObjectType", 3);
        }
    }
}
