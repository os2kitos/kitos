namespace Infrastructure.DataAccess.Mapping
{
    using Core.DomainModel.ItSystem;
    using Core.DomainModel.ItSystemUsage;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class SystemUsageCriticalityLevelMap : OptionEntityMap<SystemUsageCriticalityLevel, ItSystemUsage>
    {
        public override void Configure(EntityTypeBuilder<SystemUsageCriticalityLevel> builder)
        {
            base.Configure(builder);
            builder.ToTable("SystemUsageCriticalityLevelTypes");
        }
    }
}
