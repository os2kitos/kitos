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
            this.Property(t => t.ProjectType_Id).HasColumnName("ProjectType_Id");
            this.Property(t => t.ProjectCategory_Id).HasColumnName("ProjectCategory_Id");
            this.Property(t => t.Municipality_Id).HasColumnName("Municipality_Id");

            /*
            this.Property(t => t.ItProjectOwner_Id).HasColumnName("ItProjectOwner_Id");
            this.Property(t => t.ItProjectLeader_Id).HasColumnName("ItProjectLeader_Id");
            this.Property(t => t.PartItProjectLeader_Id).HasColumnName("PartItProjectLeader_Id");
            this.Property(t => t.Consultant_Id).HasColumnName("Consultant_Id");
             */

            // Relationships
            /*this.HasOptional(t => t.ItProjectOwner)
                .WithMany(t => t.OwnerOfItProjects)
                .HasForeignKey(d => d.ItProjectOwner_Id)
                .WillCascadeOnDelete(false);
            this.HasOptional(t => t.ItProjectLeader)
                .WithMany(t => t.LeaderOfItProjects)
                .HasForeignKey(d => d.ItProjectLeader_Id)
                .WillCascadeOnDelete(false);
            this.HasOptional(t => t.PartItProjectLeader)
                .WithMany(t => t.LeaderOfPartItProjects)
                .HasForeignKey(d => d.PartItProjectLeader_Id)
                .WillCascadeOnDelete(false);
            this.HasOptional(t => t.Consultant)
                .WithMany(t => t.ConsultantOnItProjects)
                .HasForeignKey(d => d.Consultant_Id)
                .WillCascadeOnDelete(false);*/
                
            this.HasRequired(t => t.Organization)
                .WithMany(t => t.ItProjects)
                .HasForeignKey(d => d.Municipality_Id)
                .WillCascadeOnDelete(false);
            this.HasRequired(t => t.ProjectCategory)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ProjectCategory_Id);
            this.HasRequired(t => t.ProjectType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ProjectType_Id);

        }
    }
}
