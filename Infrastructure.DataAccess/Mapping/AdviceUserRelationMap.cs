using Core.DomainModel.Advice;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    internal class AdviceUserRelationMap : EntityMap<AdviceUserRelation>
    {
        public override void Configure(EntityTypeBuilder<AdviceUserRelation> builder)
        {
            base.Configure(builder);
            builder.ToTable("AdviceUserRelations");
            builder.HasOne(x => x.ItContractRole)
                .WithMany(x => x.AdviceUserRelations)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DataProcessingRegistrationRole)
                .WithMany(x => x.AdviceUserRelations)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ItSystemRole)
                .WithMany(x => x.AdviceUserRelations)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
