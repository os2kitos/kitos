using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItProjectMap : EntityTypeConfiguration<ItProject>
    {
        public ItProjectMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Background)
                .IsRequired();

            this.Property(t => t.Note)
                .IsRequired();

            this.Property(t => t.Name)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("ItProject");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Background).HasColumnName("Background");
            this.Property(t => t.IsTransversal).HasColumnName("IsTransversal");
            this.Property(t => t.IsTermsOfReferenceApproved).HasColumnName("IsTermsOfReferenceApproved");
            this.Property(t => t.Note).HasColumnName("Note");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.ProjectTypeId).HasColumnName("ProjectTypeId");
            this.Property(t => t.ProjectCategoryId).HasColumnName("ProjectCategoryId");
            this.Property(t => t.MunicipalityId).HasColumnName("MunicipalityId");

            /*
            this.Property(t => t.ItProjectOwnerId).HasColumnName("ItProjectOwnerId");
            this.Property(t => t.ItProjectLeaderId).HasColumnName("ItProjectLeaderId");
            this.Property(t => t.PartItProjectLeaderId).HasColumnName("PartItProjectLeaderId");
            this.Property(t => t.ConsultantId).HasColumnName("ConsultantId");
             */

            // Relationships
            /*this.HasOptional(t => t.ItProjectOwner)
                .WithMany(t => t.OwnerOfItProjects)
                .HasForeignKey(d => d.ItProjectOwnerId)
                .WillCascadeOnDelete(false);
            this.HasOptional(t => t.ItProjectLeader)
                .WithMany(t => t.LeaderOfItProjects)
                .HasForeignKey(d => d.ItProjectLeaderId)
                .WillCascadeOnDelete(false);
            this.HasOptional(t => t.PartItProjectLeader)
                .WithMany(t => t.LeaderOfPartItProjects)
                .HasForeignKey(d => d.PartItProjectLeaderId)
                .WillCascadeOnDelete(false);
            this.HasOptional(t => t.Consultant)
                .WithMany(t => t.ConsultantOnItProjects)
                .HasForeignKey(d => d.ConsultantId)
                .WillCascadeOnDelete(false);*/
                
            this.HasRequired(t => t.Organization)
                .WithMany(t => t.ItProjects)
                .HasForeignKey(d => d.MunicipalityId)
                .WillCascadeOnDelete(false);
            this.HasRequired(t => t.ProjectCategory)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ProjectCategoryId);
            this.HasRequired(t => t.ProjectType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ProjectTypeId);

        }
    }
}
