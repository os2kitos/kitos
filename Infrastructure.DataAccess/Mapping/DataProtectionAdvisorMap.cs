using Core.DomainModel.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProtectionAdvisorMap : EntityMap<DataProtectionAdvisor>
    {
        public override void Configure(EntityTypeBuilder<DataProtectionAdvisor> builder)
        {
            base.Configure(builder);

            builder.Property(t => t.Cvr).HasMaxLength(10);
            builder.HasIndex(t => t.Cvr);
        }
    }
}
