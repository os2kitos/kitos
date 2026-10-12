using Core.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class AttachedOptionMap : EntityMap<AttachedOption>
    {
        public override void Configure(EntityTypeBuilder<AttachedOption> builder)
        {
            base.Configure(builder);

            // Override base required relationships - AttachedOption allows null ObjectOwner/LastChangedByUser
            builder.HasOne(t => t.ObjectOwner)
                .WithMany()
                .HasForeignKey(d => d.ObjectOwnerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.LastChangedByUser)
                .WithMany()
                .HasForeignKey(d => d.LastChangedByUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.OptionType).HasDatabaseName("UX_OptionType");
            builder.HasIndex(x => x.OptionId).HasDatabaseName("UX_OptionId");
            builder.HasIndex(x => x.ObjectId).HasDatabaseName("UX_ObjectId");
            builder.HasIndex(x => x.ObjectType).HasDatabaseName("UX_ObjectType");
        }
    }
}
