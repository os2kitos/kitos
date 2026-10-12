namespace Infrastructure.DataAccess.Mapping
{
    using Core.DomainModel.ItSystemUsage;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    using ArchiveLocation = Core.DomainModel.ItSystem.ArchiveLocation;

    public class ArchiveLocationMap : OptionEntityMap<ArchiveLocation, ItSystemUsage>
    {
        public override void Configure(EntityTypeBuilder<ArchiveLocation> builder)
        {
            base.Configure(builder);
            builder.ToTable("ArchiveLocations");
        }
    }
}
