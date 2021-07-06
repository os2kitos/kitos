using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItProjectMap : EntityMap<ItProject>
    {
        public ItProjectMap()
        {
            // Properties

            this.Property(x => x.Name)
                .HasMaxLength(ItProjectConstraints.MaxNameLength)
                .IsRequired()
                .HasIndexAnnotation("Project_Index_Name", 0);

            // Table & Column Mappings
            ToTable("ItProject");

            HasRequired(t => t.Organization)
                .WithMany(d => d.ItProjects)
                .HasForeignKey(t => t.OrganizationId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.ItProjectType)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.ItProjectTypeId);

            HasOptional(t => t.Parent)
                .WithMany(d => d.Children)
                .HasForeignKey(t => t.ParentId)
                .WillCascadeOnDelete(false);

            HasMany(t => t.UsedByOrgUnits)
                .WithRequired(t => t.ItProject)
                .HasForeignKey(d => d.ItProjectId);

            HasMany(t => t.EconomyYears)
                .WithRequired(d => d.ItProject)
                .HasForeignKey(d => d.ItProjectId)
                .WillCascadeOnDelete(true);

            HasMany(t => t.ItSystemUsages)
                .WithMany(t => t.ItProjects);

            HasMany(t => t.TaskRefs)
                .WithMany(t => t.ItProjects);

            HasOptional(t => t.JointMunicipalProject)
                .WithMany(t => t.JointMunicipalProjects)
                .HasForeignKey(d => d.JointMunicipalProjectId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.CommonPublicProject)
                .WithMany(t => t.CommonPublicProjects)
                .HasForeignKey(d => d.CommonPublicProjectId)
                .WillCascadeOnDelete(false);

            HasMany(t => t.Risks)
                .WithRequired(d => d.ItProject)
                .HasForeignKey(d => d.ItProjectId)
                .WillCascadeOnDelete(true);

            HasOptional(t => t.Reference);
            HasMany(t => t.ExternalReferences)
                .WithOptional(d => d.ItProject)
                .HasForeignKey(d => d.ItProject_Id)
                .WillCascadeOnDelete(true);

            HasOptional(t => t.ResponsibleUsage)
                .WithOptionalPrincipal(t => t.ResponsibleItProject);

            HasMany(t => t.Stakeholders)
                .WithRequired(d => d.ItProject)
                .HasForeignKey(d => d.ItProjectId)
                .WillCascadeOnDelete(true);

            HasMany(e => e.ItProjectStatusUpdates)
                .WithOptional(e => e.AssociatedItProject)
                .HasForeignKey(e => e.AssociatedItProjectId)
                .WillCascadeOnDelete(true);

            Property(x => x.Uuid)
                .IsRequired()
                .HasUniqueIndexAnnotation("UX_Project_Uuid", 0);
        }
    }
}