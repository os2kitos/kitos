

using Core.DomainModel.Organization;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataResponsibleMap : EntityMap<DataResponsible>
    {
        public DataResponsibleMap()
        {
            Property(t => t.Cvr)
                .HasMaxLength(10)
                .IsOptional()
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));
        }
    }
}
