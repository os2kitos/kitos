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
                .HasForeignKey(d => d.ObjectOwnerId)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.LastChangedByUser)
                .WithMany()
                .HasForeignKey(d => d.LastChangedByUserId)
                .WillCascadeOnDelete(false);
        }
    }
}
