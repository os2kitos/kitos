using Core.DomainModel.GDPR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProcessingRegistrationOversightDateMap : IEntityTypeConfiguration<DataProcessingRegistrationOversightDate>
    {
        public void Configure(EntityTypeBuilder<DataProcessingRegistrationOversightDate> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Parent)
                .WithMany(x => x.OversightDates)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.OversightOption)
                .WithMany()
                .HasForeignKey(x => x.OversightOptionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
