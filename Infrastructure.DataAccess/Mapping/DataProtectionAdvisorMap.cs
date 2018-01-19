using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProtectionAdvisorMap : EntityMap<DataProtectionAdvisor>
    {
        public DataProtectionAdvisorMap()
        {
            Property(t => t.Cvr)
                .HasMaxLength(10)
                .IsOptional()
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));
        }
    }
}
