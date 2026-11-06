using Core.DomainModel.ItContract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractInvoiceDateMap : IEntityTypeConfiguration<ItContractInvoiceDate>
    {
        public void Configure(EntityTypeBuilder<ItContractInvoiceDate> builder)
        {
            builder.ToTable("ItContractInvoiceDates");
            builder.HasKey(x => new { x.ItContractId, x.Period });
            builder.HasOne(x => x.ItContract)
                .WithMany(x => x.InvoiceDates)
                .HasForeignKey(x => x.ItContractId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Property(x => x.Date).HasColumnType("timestamp without time zone");
        }
    }
}
