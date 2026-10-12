using Core.DomainModel.GDPR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProcessingRegistrationRightMap : RightMap<DataProcessingRegistration, DataProcessingRegistrationRight, DataProcessingRegistrationRole>
    {
        public override void Configure(EntityTypeBuilder<DataProcessingRegistrationRight> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.User)
                .WithMany(x => x.DataProcessingRegistrationRights)
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
