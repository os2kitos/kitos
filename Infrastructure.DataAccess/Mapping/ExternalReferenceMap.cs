using Core.DomainModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ExternalReferenceMap : EntityMap<ExternalReference>
    {
        public override void Configure(EntityTypeBuilder<ExternalReference> builder)
        {
            base.Configure(builder);
            builder.ToTable("ExternalReferences");

            builder.Property(t => t.Itcontract_Id).HasColumnName("ItContract_Id");
            builder.Property(t => t.ItSystemUsage_Id).HasColumnName("ItSystemUsage_Id");
            builder.Property(t => t.ItSystem_Id).HasColumnName("ItSystem_Id");

            builder.Property(x => x.Uuid).IsRequired();
            builder.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("UX_ExternalReference_Uuid");
        }
    }
}
