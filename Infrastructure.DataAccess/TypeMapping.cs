using Core.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess
{
    public static class TypeMapping
    {
        public static void AddIndexOnAccessModifier<T>(EntityTypeBuilder<T> builder)
            where T : Entity, IHasAccessModifier
        {
            builder.HasIndex(x => x.AccessModifier).HasDatabaseName("UX_AccessModifier");
        }
    }
}
