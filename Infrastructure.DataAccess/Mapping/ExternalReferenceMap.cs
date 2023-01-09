using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
   public class ExternalReferenceMap : EntityMap<ExternalReference>
    {
        public ExternalReferenceMap() {
            ToTable("ExternalReferences");

            Property(t => t.Itcontract_Id).HasColumnName("ItContract_Id").IsOptional();
            Property(t => t.ItSystemUsage_Id).HasColumnName("ItSystemUsage_Id").IsOptional();
            Property(t => t.ItSystem_Id).HasColumnName("ItSystem_Id").IsOptional();

            Property(x => x.Uuid)
                .IsRequired()
                .HasUniqueIndexAnnotation("UX_ExternalReferences_Uuid", 0);
        }
    }
}
