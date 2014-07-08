using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public abstract class EntityMap<T> : EntityTypeConfiguration<T>
        where T : Entity
    {
        protected EntityMap()
        {
            this.HasKey(t => t.Id);

            this.HasRequired(t => t.ObjectOwner)
                .WithMany()
                .HasForeignKey(t => t.ObjectOwnerId);
        }
    }
}