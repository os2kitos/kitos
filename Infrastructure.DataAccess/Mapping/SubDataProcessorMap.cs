using Core.DomainModel.GDPR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class SubDataProcessorMap : IEntityTypeConfiguration<SubDataProcessor>
    {
        public void Configure(EntityTypeBuilder<SubDataProcessor> builder)
        {
            builder.HasKey(x => new { x.OrganizationId, x.DataProcessingRegistrationId });

            builder.HasOne(x => x.DataProcessingRegistration)
                .WithMany(x => x.AssignedSubDataProcessors)
                .HasForeignKey(x => x.DataProcessingRegistrationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Organization)
                .WithMany(x => x.SubDataProcessorRegistrations)
                .HasForeignKey(x => x.OrganizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SubDataProcessorBasisForTransfer)
                .WithMany(x => x.SubDataProcessors)
                .HasForeignKey(x => x.SubDataProcessorBasisForTransferId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.InsecureCountry)
                .WithMany(x => x.SubDataProcessors)
                .HasForeignKey(x => x.InsecureCountryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
