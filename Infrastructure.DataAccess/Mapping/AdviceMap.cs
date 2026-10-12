using Core.DomainModel.Advice;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class AdviceMap : EntityMap<Advice>
    {
        public override void Configure(EntityTypeBuilder<Advice> builder)
        {
            base.Configure(builder);
            // Table & Column Mappings
            builder.ToTable("Advice");
            builder.HasMany(a => a.AdviceSent)
                .WithOne(a => a.Advice)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(a=> a.Reciepients)
                .WithOne(ar => ar.Advice)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
