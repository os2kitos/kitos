using Core.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class PasswordResetRequestMap : EntityMap<PasswordResetRequest>
    {
        public override void Configure(EntityTypeBuilder<PasswordResetRequest> builder)
        {
            base.Configure(builder);
            base.Configure(builder);
            builder.ToTable("PasswordResetRequest");
            builder.Property(t => t.Time).HasColumnName("Time");
            builder.Property(t => t.UserId).HasColumnName("UserId");

            builder.HasOne(t => t.User)
                .WithMany(t => t.PasswordResetRequests)
                .HasForeignKey(d => d.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
