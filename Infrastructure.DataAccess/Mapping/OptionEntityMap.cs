using Core.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public abstract class OptionEntityMap<T, TReference> : EntityMap<T>
        where T : OptionEntity<TReference>
    {
        public override void Configure(EntityTypeBuilder<T> builder)
        {
            base.Configure(builder);

            builder.Property(t => t.Name)
                .HasMaxLength(OptionEntity<TReference>.MaxNameLength)
                .IsRequired();

            builder.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("UX_Option_Uuid");
        }
    }
}
