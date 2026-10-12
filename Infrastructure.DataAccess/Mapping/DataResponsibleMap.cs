using Core.DomainModel.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataResponsibleMap : EntityMap<DataResponsible>
    {
        public override void Configure(EntityTypeBuilder<DataResponsible> builder)
        {
            base.Configure(builder);

            builder.Property(t => t.Cvr).HasMaxLength(10);
            builder.HasIndex(t => t.Cvr);
        }
    }
}
