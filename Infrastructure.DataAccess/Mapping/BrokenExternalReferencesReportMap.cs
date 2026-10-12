using Core.DomainModel.Qa.References;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class BrokenExternalReferencesReportMap : IEntityTypeConfiguration<BrokenExternalReferencesReport>
    {
        public void Configure(EntityTypeBuilder<BrokenExternalReferencesReport> builder)
        {
            builder.HasMany(x => x.BrokenExternalReferences)
                .WithOne(x => x.ParentReport)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.BrokenInterfaceLinks)
                .WithOne(x => x.ParentReport)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
