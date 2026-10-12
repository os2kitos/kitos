using Core.DomainModel.ItSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ArchiveTestLocationMap : IEntityTypeConfiguration<ArchiveTestLocation>
    {
        public void Configure(EntityTypeBuilder<ArchiveTestLocation> builder)
        {
            builder.ToTable("ArchiveTestLocations");
        }
    }
}
