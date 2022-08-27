using Core.DomainModel.Advice;

namespace Infrastructure.DataAccess.Mapping
{
    internal class AdviceUserRelationMap : EntityMap<AdviceUserRelation>
    {
        public AdviceUserRelationMap()
        {
            this.ToTable("AdviceUserRelations");
            HasOptional(x => x.ItContractRole)
                .WithMany(x => x.AdviceUserRelations)
                .WillCascadeOnDelete(false);

            HasOptional(x => x.DataProcessingRegistrationRole)
                .WithMany(x => x.AdviceUserRelations)
                .WillCascadeOnDelete(false);

            HasOptional(x => x.ItSystemRole)
                .WithMany(x => x.AdviceUserRelations)
                .WillCascadeOnDelete(false);
        }
    }
}
