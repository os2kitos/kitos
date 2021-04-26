using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.BackgroundJobs;

namespace Infrastructure.DataAccess.Mapping
{
    public class PendingReadModelUpdateMap : EntityTypeConfiguration<PendingReadModelUpdate>
    {
        public PendingReadModelUpdateMap()
        {
            Property(x => x.SourceId)
                .IsRequired()
                .HasIndexAnnotation("IX_SourceId", 0);
            Property(x => x.Category)
                .IsRequired()
                .HasIndexAnnotation("IX_Category", 0);
            Property(x => x.CreatedAt)
                .IsRequired()
                .HasIndexAnnotation("IX_CreatedAt", 0);
        }
    }
}
