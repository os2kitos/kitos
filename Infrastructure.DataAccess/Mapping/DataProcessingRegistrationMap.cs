using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.GDPR;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProcessingRegistrationMap : EntityTypeConfiguration<DataProcessingRegistration>
    {
        public DataProcessingRegistrationMap()
        {
            //Simple properties
            Property(x => x.Name)
                .HasMaxLength(DataProcessingRegistrationConstraints.MaxNameLength)
                .IsRequired()
                .HasIndexAnnotation("DataProcessingRegistration_Index_Name", 0);

            //Organization relationship
            HasRequired(t => t.Organization)
                .WithMany(t => t.DataProcessingRegistrations)
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(false);

            //External references
            HasOptional(t => t.Reference);
            HasMany(t => t.ExternalReferences)
                .WithOptional(d => d.DataProcessingRegistration)
                .HasForeignKey(d => d.DataProcessingRegistration_Id)
                .WillCascadeOnDelete(true);

            //It-systems
            HasMany(x=>x.SystemUsages)
                .WithMany(x=>x.AssociatedDataProcessingRegistrations);
        }
    }
}