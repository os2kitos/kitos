using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public abstract class OptionEntityMap<T, TReference> : EntityMap<T>
        where T : OptionEntity<TReference>
    {
        protected OptionEntityMap()
        {
            this.Property(t => t.Name)
                .HasMaxLength(OptionEntity<TReference>.MaxNameLength)
                .IsRequired();
        }
    }
}
