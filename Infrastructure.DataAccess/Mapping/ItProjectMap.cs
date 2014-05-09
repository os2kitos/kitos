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

            // Table & Column Mappings
            this.ToTable("ItProject");
            this.Property(t => t.Id).HasColumnName("Id");

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

            this.HasOptional(t => t.AssociatedProgram)
                .WithMany(t => t.AssociatedProjects)
                .HasForeignKey(d => d.AssociatedProgramId)
                .WillCascadeOnDelete(false);

            this.HasMany(t => t.UsedByOrgUnits)
                .WithMany(t => t.UsingItProjects)
                .Map(mc =>
                {
                    mc.MapLeftKey("ItProjectId");
                    mc.MapRightKey("OrgUnitId");
                });

            this.HasMany(t => t.EconomyYears)
                .WithRequired(d => d.ItProject)
                .HasForeignKey(d => d.ItProjectId);

            this.HasMany(t => t.ItSystemUsages)
                .WithMany(t => t.ItProjects);

            this.HasMany(t => t.TaskRefs)
                .WithMany(t => t.ItProjects);

            this.HasOptional(t => t.JointMunicipalProject)
                .WithMany(t => t.JointMunicipalProjects)
                .HasForeignKey(d => d.JointMunicipalProjectId)
                .WillCascadeOnDelete(false);

            this.HasOptional(t => t.CommonPublicProject)
                .WithMany(t => t.CommonPublicProjects)
                .HasForeignKey(d => d.CommonPublicProjectId)
                .WillCascadeOnDelete(false);

            this.HasMany(t => t.Risks)
                .WithRequired(d => d.ItProject)
                .HasForeignKey(d => d.ItProjectId)
                .WillCascadeOnDelete(true);

            this.HasOptional(t => t.ResponsibleOrgUnit)
                .WithMany(t => t.ResponsibleForItProjects)
                .HasForeignKey(d => d.ResponsibleOrgUnitId)
                .WillCascadeOnDelete(false);

            this.HasOptional(t => t.ParentItProject)
                .WithMany(t => t.ChildItProjects)
                .HasForeignKey(d => d.ParentItProjectId)
                .WillCascadeOnDelete(false);
        }
    }
}
