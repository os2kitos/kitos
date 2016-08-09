using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public abstract class EntityMap<T> : EntityTypeConfiguration<T>
        where T : Entity
    {
        protected EntityMap()
        {
            HasKey(t => t.Id);

            HasRequired(t => t.ObjectOwner)
                .WithMany()
                .HasForeignKey(d => d.ObjectOwnerId)
                .WillCascadeOnDelete(false);

            HasRequired(t => t.LastChangedByUser)
                .WithMany()
                .HasForeignKey(d => d.LastChangedByUserId)
                .WillCascadeOnDelete(false);
        }
    }
}
