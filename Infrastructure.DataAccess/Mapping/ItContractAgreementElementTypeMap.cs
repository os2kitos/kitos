using Core.DomainModel.ItContract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractAgreementElementTypeMap : IEntityTypeConfiguration<ItContractAgreementElementTypes>
    {
        public void Configure(EntityTypeBuilder<ItContractAgreementElementTypes> builder)
        {
            builder.ToTable("ItContractAgreementElementTypes");
            builder.Property(t => t.ItContract_Id).HasColumnName("ItContract_Id");
            builder.Property(t => t.AgreementElementType_Id).HasColumnName("AgreementElementType_Id");

            builder.HasKey(x => new { x.AgreementElementType_Id, x.ItContract_Id });
        }
    }
}
