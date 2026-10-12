using Core.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class KendoColumnConfigurationMap : IEntityTypeConfiguration<KendoColumnConfiguration>
    {
        public void Configure(EntityTypeBuilder<KendoColumnConfiguration> builder)
        {
            builder.ToTable("KendoColumnConfigurations");
        }
    }
}
