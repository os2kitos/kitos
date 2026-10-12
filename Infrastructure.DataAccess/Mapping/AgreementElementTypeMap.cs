using Core.DomainModel.ItContract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class AgreementElementTypeMap : OptionEntityMap<AgreementElementType, ItContract>
    {
        public override void Configure(EntityTypeBuilder<AgreementElementType> builder)
        {
            base.Configure(builder);

            builder.HasMany(t => t.References)
                .WithOne(t => t.AgreementElementType)
                .HasForeignKey(d => d.AgreementElementType_Id)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
