using Core.DomainModel.ItContract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class EconomyStreamMap : EntityMap<EconomyStream>
    {
        public override void Configure(EntityTypeBuilder<EconomyStream> builder)
        {
            base.Configure(builder);
            builder.ToTable("EconomyStream");

            builder.HasOne(t => t.ExternPaymentFor)
                .WithMany(d => d.ExternEconomyStreams)
                .HasForeignKey(t => t.ExternPaymentForId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.InternPaymentFor)
                .WithMany(d => d.InternEconomyStreams)
                .HasForeignKey(t => t.InternPaymentForId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.OrganizationUnit)
                .WithMany(d => d.EconomyStreams)
                .HasForeignKey(t => t.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
