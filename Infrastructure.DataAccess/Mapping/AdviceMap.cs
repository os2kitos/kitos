using Core.DomainModel.Advice;

namespace Infrastructure.DataAccess.Mapping
{
    public class AdviceMap : EntityMap<Advice>
    {
        public AdviceMap()
        {
            // Table & Column Mappings
            this.ToTable("Advice");
            this.HasMany(a => a.AdviceSent)
                .WithRequired(a => a.Advice)
                .WillCascadeOnDelete(true);
            this.HasMany(a=> a.Reciepients)
                .WithRequired(ar => ar.Advice)
                .WillCascadeOnDelete(true);
        }
    }
}
