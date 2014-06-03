using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItProjectMap : EntityMap<ItProject>
    {
        public ItProjectMap()
        {
            // Table & Column Mappings
            this.ToTable("ItProject");
                
            this.HasRequired(t => t.Organization)
                .WithMany(d => d.ItProjects)
                .HasForeignKey(t => t.OrganizationId)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.ItProjectCategory)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.ItProjectCategoryId);

            this.HasRequired(t => t.ItProjectType)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.ItProjectTypeId);

            this.HasOptional(t => t.AssociatedProgram)
                .WithMany(d => d.AssociatedProjects)
                .HasForeignKey(t => t.AssociatedProgramId)
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
                .HasForeignKey(d => d.ItProjectId)
                .WillCascadeOnDelete(true);

            this.HasMany(t => t.ItSystemUsages)
                .WithMany(t => t.ItProjects);

            this.HasMany(t => t.TaskRefs)
                .WithMany(t => t.ItProjects);

            this.HasRequired(t => t.Phase1)
                .WithOptional(d => d.Phase1ForProject)
                .Map(mc => mc.MapKey("Phase1Id"));

            this.HasRequired(t => t.Phase2)
                .WithOptional(d => d.Phase2ForProject)
                .Map(mc => mc.MapKey("Phase2Id"));

            this.HasRequired(t => t.Phase3)
                .WithOptional(d => d.Phase3ForProject)
                .Map(mc => mc.MapKey("Phase3Id"));

            this.HasRequired(t => t.Phase4)
                .WithOptional(d => d.Phase4ForProject)
                .Map(mc => mc.MapKey("Phase4Id"));

            this.HasRequired(t => t.Phase5)
                .WithOptional(d => d.Phase5ForProject)
                .Map(mc => mc.MapKey("Phase5Id"));

            this.HasMany(t => t.TaskActivities)
                .WithOptional(d => d.TaskForProject)
                .HasForeignKey(d => d.TaskForProjectId)
                .WillCascadeOnDelete(true);

            this.HasMany(t => t.MilestoneStates)
                .WithOptional(d => d.MilestoneForProject)
                .HasForeignKey(d => d.MilestoneForProjectId)
                .WillCascadeOnDelete(true);

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

            this.HasMany(t => t.Stakeholders)
                .WithRequired(d => d.ItProject)
                .HasForeignKey(d => d.ItProjectId)
                .WillCascadeOnDelete(true);
        }
    }
}
