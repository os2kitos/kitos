using Core.DomainModel.Advice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class AdviceSentMap : EntityMap<AdviceSent>
    {
        public override void Configure(EntityTypeBuilder<AdviceSent> builder)
        {
            base.Configure(builder);
            builder.ToTable("AdviceSents");
        }
    }
}
