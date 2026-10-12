using Core.DomainModel.Qa.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class BrokenLinkInExternalReferenceMap : IEntityTypeConfiguration<BrokenLinkInExternalReference>
    {
        public void Configure(EntityTypeBuilder<BrokenLinkInExternalReference> builder)
        {
            builder.ToTable("BrokenLinkInExternalReferences");

            builder.HasOne(x => x.BrokenReferenceOrigin)
                .WithMany(x => x.BrokenLinkReports)
                .HasForeignKey("BrokenReferenceOrigin_Id")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ParentReport)
                .WithMany(x => x.BrokenExternalReferences)
                .HasForeignKey("ParentReport_Id")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
