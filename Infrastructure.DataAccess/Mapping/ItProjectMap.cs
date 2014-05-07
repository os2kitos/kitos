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
                .HasForeignKey(d => d.ItProjectId);

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
                .HasForeignKey(d => d.TaskForProjectId);

        }
    }
}
