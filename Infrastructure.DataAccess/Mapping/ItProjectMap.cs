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

            this.HasRequired(t => t.ItProjectType)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.ItProjectTypeId);

           this.HasOptional(t => t.Parent)
                .WithMany(d => d.Children)
                .HasForeignKey(t => t.ParentId)
                .WillCascadeOnDelete(false);

            //this.HasMany(t => t.UsedByOrgUnits)
            //    .WithMany(t => t.UsingItProjects)
            //    .Map(mc =>
            //    {
            //        mc.MapLeftKey("ItProjectId");
            //        mc.MapRightKey("OrgUnitId");
            //    });

            this.HasMany(t => t.EconomyYears)
                .WithRequired(d => d.ItProject)
                .HasForeignKey(d => d.ItProjectId)
                .WillCascadeOnDelete(true);

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

            //this.HasOptional(t => t.ResponsibleOrgUnit)
            //    .WithMany(t => t.ResponsibleForItProjects)
            //    .HasForeignKey(d => d.ResponsibleOrgUnitId)
            //    .WillCascadeOnDelete(false);

            this.HasOptional(t => t.Original)
                .WithMany(t => t.Clones)
                .HasForeignKey(d => d.OriginalId)
                .WillCascadeOnDelete(false);

            this.HasMany(t => t.Stakeholders)
                .WithRequired(d => d.ItProject)
                .HasForeignKey(d => d.ItProjectId)
                .WillCascadeOnDelete(true);
        }
    }
}
