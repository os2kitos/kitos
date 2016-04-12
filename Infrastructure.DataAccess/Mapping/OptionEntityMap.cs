using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public abstract class OptionEntityMap<T, TReference> : EntityMap<T>
        where T : Entity, IOptionEntity<TReference>
    {
        protected OptionEntityMap()
        {
            this.Property(t => t.Name)
                .IsRequired();
        }
    }
}
