using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItInterfaceMap : EntityMap<ItInterface>
    {
        public ItInterfaceMap()
        {
            // Properties
            this.Property(x => x.Version).HasMaxLength(20);

            // BUG there's an issue with indexing http://stackoverflow.com/questions/26055140/ef-migrations-drops-index-when-adding-compsite-index
            this.Property(x => x.OrganizationId)
                .HasUniqueIndexAnnotation("UX_NamePerOrg", 0);
            this.Property(x => x.Name)
                .HasMaxLength(ItInterface.MaxNameLength) // http://stackoverflow.com/questions/1827063/mysql-error-key-specification-without-a-key-length
                .IsRequired()
                .HasUniqueIndexAnnotation("UX_NamePerOrg", 1);
            this.Property(x => x.ItInterfaceId)
                .HasMaxLength(100)
                // this should really be optional but because
                // MySql doesn't follow the SQL standard
                // when it comes to unique indexs with nulls in them - we can't...
                // http://bugs.mysql.com/bug.php?id=8173
                // So instead we set it to an empty string :´(
                // TODO we're no longer using MySql so we can fix this
                .IsRequired()
                .HasUniqueIndexAnnotation("UX_NamePerOrg", 2);

            // Table & Column Mappings
            this.ToTable("ItInterface");

            // Relationships
            // Udkommenteret ifm. OS2KITOS-663
            //this.HasMany(t => t.CanBeUsedBy)
            //    .WithRequired(t => t.ItInterface)
            //    .HasForeignKey(d => d.ItInterfaceId);

            this.HasOptional(t => t.Interface)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.InterfaceId);

            this.HasRequired(t => t.Organization)
                .WithMany(d => d.ItInterfaces)
                .HasForeignKey(t => t.OrganizationId)
                .WillCascadeOnDelete(false);

            TypeMapping.AddIndexOnAccessModifier<ItInterfaceMap, ItInterface>(this);
        }
    }
}
