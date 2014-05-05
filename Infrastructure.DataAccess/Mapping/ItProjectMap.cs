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
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(false);
            this.HasRequired(t => t.ItProjectCategory)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ItProjectCategoryId);
            this.HasRequired(t => t.ItProjectType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ItProjectTypeId);

        }
    }
}
