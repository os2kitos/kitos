using Core.DomainModel.LocalOptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class LocalRelationFrequencyTypeMap : IEntityTypeConfiguration<LocalRelationFrequencyType>
    {
        public void Configure(EntityTypeBuilder<LocalRelationFrequencyType> builder)
        {
            builder.ToTable("LocalRelationFrequencyTypes");
        }
    }
}
