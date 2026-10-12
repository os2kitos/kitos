using Core.DomainModel.BackgroundJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class PendingReadModelUpdateMap : IEntityTypeConfiguration<PendingReadModelUpdate>
    {
        public void Configure(EntityTypeBuilder<PendingReadModelUpdate> builder)
        {
            builder.Property(x => x.SourceId).IsRequired();
            builder.HasIndex(x => x.SourceId).HasDatabaseName("IX_SourceId");

            builder.Property(x => x.Category).IsRequired();
            builder.HasIndex(x => x.Category).HasDatabaseName("IX_Category");

            builder.Property(x => x.CreatedAt).IsRequired();
            builder.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_CreatedAt");

            builder.HasIndex(x => new { x.Category, x.Id }).HasDatabaseName("IX_Category_Id");
        }
    }
}
