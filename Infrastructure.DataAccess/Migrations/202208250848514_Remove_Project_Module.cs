namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_Project_Module : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItProject", "CommonPublicProjectId", "dbo.ItProject");
            DropForeignKey("dbo.Communication", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.Communication", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Communication", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Communication", "ResponsibleUserId", "dbo.User");
            DropForeignKey("dbo.EconomyYears", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.EconomyYears", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.EconomyYears", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ExternalReferences", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.Goal", "GoalStatusId", "dbo.GoalStatus");
            DropForeignKey("dbo.GoalTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.GoalTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Goal", "GoalTypeId", "dbo.GoalTypes");
            DropForeignKey("dbo.Goal", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Goal", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.GoalStatus", "Id", "dbo.ItProject");
            DropForeignKey("dbo.GoalStatus", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.GoalStatus", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Handover", "Id", "dbo.ItProject");
            DropForeignKey("dbo.Handover", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Handover", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.HandoverUsers", "Handover_Id", "dbo.Handover");
            DropForeignKey("dbo.HandoverUsers", "User_Id", "dbo.User");
            DropForeignKey("dbo.ItProjectStatus", "AssociatedItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectStatus", "AssociatedUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectStatus", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectStatus", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProjectStatusUpdates", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectStatusUpdates", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProjectStatusUpdates", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItProjectStatusUpdates", "AssociatedItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProject", "ItProjectTypeId", "dbo.ItProjectTypes");
            DropForeignKey("dbo.ItProjectItSystemUsages", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectItSystemUsages", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItProject", "JointMunicipalProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProject", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProject", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItProject", "ParentId", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "ReferenceId", "dbo.ExternalReferences");
            DropForeignKey("dbo.ItProjectOrgUnitUsages", "OrganizationUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.ItProjectOrgUnitUsages", "ResponsibleItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectRights", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectRights", "ObjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectRights", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.AdviceUserRelations", "ItProjectRoleId", "dbo.ItProjectRoles");
            DropForeignKey("dbo.ItProjectRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProjectRights", "RoleId", "dbo.ItProjectRoles");
            DropForeignKey("dbo.ItProjectRights", "UserId", "dbo.User");
            DropForeignKey("dbo.Risk", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Risk", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Risk", "ResponsibleUserId", "dbo.User");
            DropForeignKey("dbo.Risk", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.Stakeholder", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Stakeholder", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Stakeholder", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectTaskRefs", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectTaskRefs", "TaskRef_Id", "dbo.TaskRef");
            DropForeignKey("dbo.ItProjectOrgUnitUsages", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.UserNotifications", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.ItSystemUsageOverviewItProjectReadModels", "ParentId", "dbo.ItSystemUsageOverviewReadModels");
            DropForeignKey("dbo.LocalGoalTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalGoalTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalGoalTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalItProjectRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalItProjectRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalItProjectRoles", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalItProjectTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalItProjectTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalItProjectTypes", "OrganizationId", "dbo.Organization");
            DropIndex("dbo.ExternalReferences", new[] { "ItProject_Id" });
            DropIndex("dbo.ItProject", "UX_Project_Uuid");
            DropIndex("dbo.ItProject", new[] { "JointMunicipalProjectId" });
            DropIndex("dbo.ItProject", new[] { "CommonPublicProjectId" });
            DropIndex("dbo.ItProject", "UX_NameUniqueToOrg");
            DropIndex("dbo.ItProject", new[] { "Name" });
            DropIndex("dbo.ItProject", new[] { "ParentId" });
            DropIndex("dbo.ItProject", new[] { "ItProjectTypeId" });
            DropIndex("dbo.ItProject", new[] { "OrganizationId" });
            DropIndex("dbo.ItProject", new[] { "ReferenceId" });
            DropIndex("dbo.ItProject", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProject", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Communication", new[] { "ResponsibleUserId" });
            DropIndex("dbo.Communication", new[] { "ItProjectId" });
            DropIndex("dbo.Communication", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Communication", new[] { "LastChangedByUserId" });
            DropIndex("dbo.EconomyYears", new[] { "ItProjectId" });
            DropIndex("dbo.EconomyYears", new[] { "ObjectOwnerId" });
            DropIndex("dbo.EconomyYears", new[] { "LastChangedByUserId" });
            DropIndex("dbo.GoalStatus", new[] { "Id" });
            DropIndex("dbo.GoalStatus", new[] { "ObjectOwnerId" });
            DropIndex("dbo.GoalStatus", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Goal", new[] { "GoalTypeId" });
            DropIndex("dbo.Goal", new[] { "GoalStatusId" });
            DropIndex("dbo.Goal", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Goal", new[] { "LastChangedByUserId" });
            DropIndex("dbo.GoalTypes", "UX_Option_Uuid");
            DropIndex("dbo.GoalTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.GoalTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Handover", new[] { "Id" });
            DropIndex("dbo.Handover", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Handover", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProjectStatus", new[] { "AssociatedUserId" });
            DropIndex("dbo.ItProjectStatus", new[] { "AssociatedItProjectId" });
            DropIndex("dbo.ItProjectStatus", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProjectStatus", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProjectStatusUpdates", new[] { "AssociatedItProjectId" });
            DropIndex("dbo.ItProjectStatusUpdates", new[] { "OrganizationId" });
            DropIndex("dbo.ItProjectStatusUpdates", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProjectStatusUpdates", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProjectTypes", "UX_Option_Uuid");
            DropIndex("dbo.ItProjectTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProjectTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProjectOrgUnitUsages", new[] { "ItProjectId" });
            DropIndex("dbo.ItProjectOrgUnitUsages", new[] { "OrganizationUnitId" });
            DropIndex("dbo.ItProjectOrgUnitUsages", new[] { "ResponsibleItProject_Id" });
            DropIndex("dbo.ItProjectRights", new[] { "UserId" });
            DropIndex("dbo.ItProjectRights", new[] { "RoleId" });
            DropIndex("dbo.ItProjectRights", new[] { "ObjectId" });
            DropIndex("dbo.ItProjectRights", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProjectRights", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProjectRoles", "UX_Option_Uuid");
            DropIndex("dbo.ItProjectRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProjectRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "ItProjectRoleId" });
            DropIndex("dbo.Risk", new[] { "ItProjectId" });
            DropIndex("dbo.Risk", new[] { "ResponsibleUserId" });
            DropIndex("dbo.Risk", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Risk", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Stakeholder", new[] { "ItProjectId" });
            DropIndex("dbo.Stakeholder", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Stakeholder", new[] { "LastChangedByUserId" });
            DropIndex("dbo.UserNotifications", new[] { "ItProject_Id" });
            DropIndex("dbo.ItSystemUsageOverviewItProjectReadModels", "ItSystemUsageOverviewItProjectReadModel_index_ItProjectId");
            DropIndex("dbo.ItSystemUsageOverviewItProjectReadModels", "ItSystemUsageOverviewItProjectReadModel_index_ItProjectName");
            DropIndex("dbo.ItSystemUsageOverviewItProjectReadModels", new[] { "ParentId" });
            DropIndex("dbo.LocalGoalTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalGoalTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalGoalTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalItProjectRoles", new[] { "OrganizationId" });
            DropIndex("dbo.LocalItProjectRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalItProjectRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalItProjectTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalItProjectTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalItProjectTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.HandoverUsers", new[] { "Handover_Id" });
            DropIndex("dbo.HandoverUsers", new[] { "User_Id" });
            DropIndex("dbo.ItProjectItSystemUsages", new[] { "ItProject_Id" });
            DropIndex("dbo.ItProjectItSystemUsages", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.ItProjectTaskRefs", new[] { "ItProject_Id" });
            DropIndex("dbo.ItProjectTaskRefs", new[] { "TaskRef_Id" });
            DropColumn("dbo.ExternalReferences", "ItProject_Id");
            DropColumn("dbo.AdviceUserRelations", "ItProjectRoleId");
            DropColumn("dbo.UserNotifications", "ItProject_Id");
            DropColumn("dbo.Config", "ShowItProjectModule");
            DropColumn("dbo.Config", "ShowItProjectPrefix");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "ItProjectNamesAsCsv");
            DropTable("dbo.ItProject");
            DropTable("dbo.Communication");
            DropTable("dbo.EconomyYears");
            DropTable("dbo.GoalStatus");
            DropTable("dbo.Goal");
            DropTable("dbo.GoalTypes");
            DropTable("dbo.Handover");
            DropTable("dbo.ItProjectStatus");
            DropTable("dbo.ItProjectStatusUpdates");
            DropTable("dbo.ItProjectTypes");
            DropTable("dbo.ItProjectOrgUnitUsages");
            DropTable("dbo.ItProjectRights");
            DropTable("dbo.ItProjectRoles");
            DropTable("dbo.Risk");
            DropTable("dbo.Stakeholder");
            DropTable("dbo.ItSystemUsageOverviewItProjectReadModels");
            DropTable("dbo.LocalGoalTypes");
            DropTable("dbo.LocalItProjectRoles");
            DropTable("dbo.LocalItProjectTypes");
            DropTable("dbo.HandoverUsers");
            DropTable("dbo.ItProjectItSystemUsages");
            DropTable("dbo.ItProjectTaskRefs");

            // Reset default user start preference for users who used to prefer it-projects
            Sql(@"UPDATE [dbo].[User] 
                    SET DefaultUserStartPreference = NULL 
                    WHERE DefaultUserStartPreference = 'it-project.overview';");

            // Delete all project advices
            Sql(@"DELETE dbo.Advice 
                    WHERE Type = 2;"); //2 = project

            // Delete delta entries for life cycle tracking events
            Sql(@"DELETE dbo.LifeCycleTrackingEvents 
                    WHERE EntityType = 5;"); // 5 = Project

            // Delete ProjectAdmin role assignments
            Sql(@"DELETE dbo.OrganizationRights 
                    WHERE Role = 3;"); // 3 = ItProjectModuleAdmin

            // Delete ProjectAdmin role assignments
            Sql(@"DELETE dbo.PendingReadModelUpdates 
                    WHERE Category = 16;"); //16 = ItSystemUsage_Project
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ItProjectTaskRefs",
                c => new
                    {
                        ItProject_Id = c.Int(nullable: false),
                        TaskRef_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItProject_Id, t.TaskRef_Id });
            
            CreateTable(
                "dbo.ItProjectItSystemUsages",
                c => new
                    {
                        ItProject_Id = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItProject_Id, t.ItSystemUsage_Id });
            
            CreateTable(
                "dbo.HandoverUsers",
                c => new
                    {
                        Handover_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Handover_Id, t.User_Id });
            
            CreateTable(
                "dbo.LocalItProjectTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LocalItProjectRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LocalGoalTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItSystemUsageOverviewItProjectReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.Int(nullable: false),
                        ItProjectName = c.String(nullable: false, maxLength: 150),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Stakeholder",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.Int(nullable: false),
                        Name = c.String(),
                        Role = c.String(),
                        Downsides = c.String(),
                        Benefits = c.String(),
                        Significance = c.Int(nullable: false),
                        HowToHandle = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Risk",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.Int(nullable: false),
                        Name = c.String(),
                        Action = c.String(),
                        Probability = c.Int(nullable: false),
                        Consequence = c.Int(nullable: false),
                        ResponsibleUserId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItProjectRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 150),
                        IsLocallyAvailable = c.Boolean(nullable: false),
                        IsObligatory = c.Boolean(nullable: false),
                        Description = c.String(),
                        IsEnabled = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        Uuid = c.Guid(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItProjectRights",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        ObjectId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItProjectOrgUnitUsages",
                c => new
                    {
                        ItProjectId = c.Int(nullable: false),
                        OrganizationUnitId = c.Int(nullable: false),
                        ResponsibleItProject_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItProjectId, t.OrganizationUnitId });
            
            CreateTable(
                "dbo.ItProjectTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 150),
                        IsLocallyAvailable = c.Boolean(nullable: false),
                        IsObligatory = c.Boolean(nullable: false),
                        Description = c.String(),
                        IsEnabled = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        Uuid = c.Guid(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItProjectStatusUpdates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssociatedItProjectId = c.Int(),
                        IsCombined = c.Boolean(nullable: false),
                        Note = c.String(),
                        TimeStatus = c.Int(nullable: false),
                        QualityStatus = c.Int(nullable: false),
                        ResourcesStatus = c.Int(nullable: false),
                        CombinedStatus = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        OrganizationId = c.Int(nullable: false),
                        IsFinal = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItProjectStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HumanReadableId = c.String(),
                        Name = c.String(),
                        Description = c.String(),
                        Note = c.String(),
                        TimeEstimate = c.Int(nullable: false),
                        AssociatedUserId = c.Int(),
                        AssociatedItProjectId = c.Int(nullable: false),
                        AssociatedPhaseNum = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                        StartDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        EndDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        StatusProcentage = c.Int(),
                        Date = c.DateTime(precision: 7, storeType: "datetime2"),
                        Status = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Handover",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Description = c.String(),
                        MeetingDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        Summary = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GoalTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 150),
                        IsLocallyAvailable = c.Boolean(nullable: false),
                        IsObligatory = c.Boolean(nullable: false),
                        Description = c.String(),
                        IsEnabled = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        Uuid = c.Guid(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Goal",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HumanReadableId = c.String(),
                        Name = c.String(),
                        Description = c.String(),
                        Note = c.String(),
                        Measurable = c.Boolean(nullable: false),
                        Status = c.Int(nullable: false),
                        GoalTypeId = c.Int(),
                        GoalStatusId = c.Int(nullable: false),
                        SubGoalDate1 = c.DateTime(precision: 7, storeType: "datetime2"),
                        SubGoalDate2 = c.DateTime(precision: 7, storeType: "datetime2"),
                        SubGoalDate3 = c.DateTime(precision: 7, storeType: "datetime2"),
                        SubGoal1 = c.String(),
                        SubGoal2 = c.String(),
                        SubGoal3 = c.String(),
                        SubGoalRea1 = c.String(),
                        SubGoalRea2 = c.String(),
                        SubGoalRea3 = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GoalStatus",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        StatusDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        StatusNote = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EconomyYears",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        YearNumber = c.Int(nullable: false),
                        ItProjectId = c.Int(nullable: false),
                        ConsultantBudget = c.Int(nullable: false),
                        ConsultantRea = c.Int(nullable: false),
                        EducationBudget = c.Int(nullable: false),
                        EducationRea = c.Int(nullable: false),
                        OtherBusinessExpensesBudget = c.Int(nullable: false),
                        OtherBusinessExpensesRea = c.Int(nullable: false),
                        IncreasedBusinessExpensesBudget = c.Int(nullable: false),
                        IncreasedBusinessExpensesRea = c.Int(nullable: false),
                        HardwareBudget = c.Int(nullable: false),
                        HardwareRea = c.Int(nullable: false),
                        SoftwareBudget = c.Int(nullable: false),
                        SoftwareRea = c.Int(nullable: false),
                        OtherItExpensesBudget = c.Int(nullable: false),
                        OtherItExpensesRea = c.Int(nullable: false),
                        IncreasedItExpensesBudget = c.Int(nullable: false),
                        IncreasedItExpensesRea = c.Int(nullable: false),
                        SalaryBudget = c.Int(nullable: false),
                        SalaryRea = c.Int(nullable: false),
                        OtherBusinessSavingsBudget = c.Int(nullable: false),
                        OtherBusinessSavingsRea = c.Int(nullable: false),
                        LicenseSavingsBudget = c.Int(nullable: false),
                        LicenseSavingsRea = c.Int(nullable: false),
                        SystemMaintenanceSavingsBudget = c.Int(nullable: false),
                        SystemMaintenanceSavingsRea = c.Int(nullable: false),
                        OtherItSavingsBudget = c.Int(nullable: false),
                        OtherItSavingsRea = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Communication",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TargetAudiance = c.String(),
                        Purpose = c.String(),
                        Message = c.String(),
                        Media = c.String(),
                        DueDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        ResponsibleUserId = c.Int(),
                        ItProjectId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItProject",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Uuid = c.Guid(nullable: false),
                        IsTransversal = c.Boolean(nullable: false),
                        IsStatusGoalVisible = c.Boolean(nullable: false),
                        IsStrategyVisible = c.Boolean(nullable: false),
                        IsRiskVisible = c.Boolean(nullable: false),
                        IsHierarchyVisible = c.Boolean(nullable: false),
                        IsEconomyVisible = c.Boolean(nullable: false),
                        IsStakeholderVisible = c.Boolean(nullable: false),
                        IsCommunicationVisible = c.Boolean(nullable: false),
                        IsHandoverVisible = c.Boolean(nullable: false),
                        IsStrategy = c.Boolean(nullable: false),
                        JointMunicipalProjectId = c.Int(),
                        CommonPublicProjectId = c.Int(),
                        ItProjectId = c.String(),
                        Background = c.String(),
                        Note = c.String(),
                        Name = c.String(nullable: false, maxLength: 150),
                        Description = c.String(),
                        IsArchived = c.Boolean(nullable: false),
                        Esdh = c.String(),
                        Cmdb = c.String(),
                        Folder = c.String(),
                        ParentId = c.Int(),
                        ItProjectTypeId = c.Int(),
                        OrganizationId = c.Int(nullable: false),
                        ReferenceId = c.Int(),
                        Priority = c.Int(nullable: false),
                        IsPriorityLocked = c.Boolean(nullable: false),
                        PriorityPf = c.Int(nullable: false),
                        StatusDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        StatusNote = c.String(),
                        Phase1_Name = c.String(),
                        Phase1_StartDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        Phase1_EndDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        Phase2_Name = c.String(),
                        Phase2_StartDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        Phase2_EndDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        Phase3_Name = c.String(),
                        Phase3_StartDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        Phase3_EndDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        Phase4_Name = c.String(),
                        Phase4_StartDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        Phase4_EndDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        Phase5_Name = c.String(),
                        Phase5_StartDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        Phase5_EndDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        CurrentPhase = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "ItProjectNamesAsCsv", c => c.String());
            AddColumn("dbo.Config", "ShowItProjectPrefix", c => c.Boolean(nullable: false));
            AddColumn("dbo.Config", "ShowItProjectModule", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserNotifications", "ItProject_Id", c => c.Int());
            AddColumn("dbo.AdviceUserRelations", "ItProjectRoleId", c => c.Int());
            AddColumn("dbo.ExternalReferences", "ItProject_Id", c => c.Int());
            CreateIndex("dbo.ItProjectTaskRefs", "TaskRef_Id");
            CreateIndex("dbo.ItProjectTaskRefs", "ItProject_Id");
            CreateIndex("dbo.ItProjectItSystemUsages", "ItSystemUsage_Id");
            CreateIndex("dbo.ItProjectItSystemUsages", "ItProject_Id");
            CreateIndex("dbo.HandoverUsers", "User_Id");
            CreateIndex("dbo.HandoverUsers", "Handover_Id");
            CreateIndex("dbo.LocalItProjectTypes", "LastChangedByUserId");
            CreateIndex("dbo.LocalItProjectTypes", "ObjectOwnerId");
            CreateIndex("dbo.LocalItProjectTypes", "OrganizationId");
            CreateIndex("dbo.LocalItProjectRoles", "LastChangedByUserId");
            CreateIndex("dbo.LocalItProjectRoles", "ObjectOwnerId");
            CreateIndex("dbo.LocalItProjectRoles", "OrganizationId");
            CreateIndex("dbo.LocalGoalTypes", "LastChangedByUserId");
            CreateIndex("dbo.LocalGoalTypes", "ObjectOwnerId");
            CreateIndex("dbo.LocalGoalTypes", "OrganizationId");
            CreateIndex("dbo.ItSystemUsageOverviewItProjectReadModels", "ParentId");
            CreateIndex("dbo.ItSystemUsageOverviewItProjectReadModels", "ItProjectName", name: "ItSystemUsageOverviewItProjectReadModel_index_ItProjectName");
            CreateIndex("dbo.ItSystemUsageOverviewItProjectReadModels", "ItProjectId", name: "ItSystemUsageOverviewItProjectReadModel_index_ItProjectId");
            CreateIndex("dbo.UserNotifications", "ItProject_Id");
            CreateIndex("dbo.Stakeholder", "LastChangedByUserId");
            CreateIndex("dbo.Stakeholder", "ObjectOwnerId");
            CreateIndex("dbo.Stakeholder", "ItProjectId");
            CreateIndex("dbo.Risk", "LastChangedByUserId");
            CreateIndex("dbo.Risk", "ObjectOwnerId");
            CreateIndex("dbo.Risk", "ResponsibleUserId");
            CreateIndex("dbo.Risk", "ItProjectId");
            CreateIndex("dbo.AdviceUserRelations", "ItProjectRoleId");
            CreateIndex("dbo.ItProjectRoles", "LastChangedByUserId");
            CreateIndex("dbo.ItProjectRoles", "ObjectOwnerId");
            CreateIndex("dbo.ItProjectRoles", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.ItProjectRights", "LastChangedByUserId");
            CreateIndex("dbo.ItProjectRights", "ObjectOwnerId");
            CreateIndex("dbo.ItProjectRights", "ObjectId");
            CreateIndex("dbo.ItProjectRights", "RoleId");
            CreateIndex("dbo.ItProjectRights", "UserId");
            CreateIndex("dbo.ItProjectOrgUnitUsages", "ResponsibleItProject_Id");
            CreateIndex("dbo.ItProjectOrgUnitUsages", "OrganizationUnitId");
            CreateIndex("dbo.ItProjectOrgUnitUsages", "ItProjectId");
            CreateIndex("dbo.ItProjectTypes", "LastChangedByUserId");
            CreateIndex("dbo.ItProjectTypes", "ObjectOwnerId");
            CreateIndex("dbo.ItProjectTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.ItProjectStatusUpdates", "LastChangedByUserId");
            CreateIndex("dbo.ItProjectStatusUpdates", "ObjectOwnerId");
            CreateIndex("dbo.ItProjectStatusUpdates", "OrganizationId");
            CreateIndex("dbo.ItProjectStatusUpdates", "AssociatedItProjectId");
            CreateIndex("dbo.ItProjectStatus", "LastChangedByUserId");
            CreateIndex("dbo.ItProjectStatus", "ObjectOwnerId");
            CreateIndex("dbo.ItProjectStatus", "AssociatedItProjectId");
            CreateIndex("dbo.ItProjectStatus", "AssociatedUserId");
            CreateIndex("dbo.Handover", "LastChangedByUserId");
            CreateIndex("dbo.Handover", "ObjectOwnerId");
            CreateIndex("dbo.Handover", "Id");
            CreateIndex("dbo.GoalTypes", "LastChangedByUserId");
            CreateIndex("dbo.GoalTypes", "ObjectOwnerId");
            CreateIndex("dbo.GoalTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.Goal", "LastChangedByUserId");
            CreateIndex("dbo.Goal", "ObjectOwnerId");
            CreateIndex("dbo.Goal", "GoalStatusId");
            CreateIndex("dbo.Goal", "GoalTypeId");
            CreateIndex("dbo.GoalStatus", "LastChangedByUserId");
            CreateIndex("dbo.GoalStatus", "ObjectOwnerId");
            CreateIndex("dbo.GoalStatus", "Id");
            CreateIndex("dbo.EconomyYears", "LastChangedByUserId");
            CreateIndex("dbo.EconomyYears", "ObjectOwnerId");
            CreateIndex("dbo.EconomyYears", "ItProjectId");
            CreateIndex("dbo.Communication", "LastChangedByUserId");
            CreateIndex("dbo.Communication", "ObjectOwnerId");
            CreateIndex("dbo.Communication", "ItProjectId");
            CreateIndex("dbo.Communication", "ResponsibleUserId");
            CreateIndex("dbo.ItProject", "LastChangedByUserId");
            CreateIndex("dbo.ItProject", "ObjectOwnerId");
            CreateIndex("dbo.ItProject", "ReferenceId");
            CreateIndex("dbo.ItProject", "OrganizationId");
            CreateIndex("dbo.ItProject", "ItProjectTypeId");
            CreateIndex("dbo.ItProject", "ParentId");
            CreateIndex("dbo.ItProject", "Name");
            CreateIndex("dbo.ItProject", new[] { "OrganizationId", "Name" }, unique: true, name: "UX_NameUniqueToOrg");
            CreateIndex("dbo.ItProject", "CommonPublicProjectId");
            CreateIndex("dbo.ItProject", "JointMunicipalProjectId");
            CreateIndex("dbo.ItProject", "Uuid", unique: true, name: "UX_Project_Uuid");
            CreateIndex("dbo.ExternalReferences", "ItProject_Id");
            AddForeignKey("dbo.LocalItProjectTypes", "OrganizationId", "dbo.Organization", "Id", cascadeDelete: true);
            AddForeignKey("dbo.LocalItProjectTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.LocalItProjectTypes", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.LocalItProjectRoles", "OrganizationId", "dbo.Organization", "Id", cascadeDelete: true);
            AddForeignKey("dbo.LocalItProjectRoles", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.LocalItProjectRoles", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.LocalGoalTypes", "OrganizationId", "dbo.Organization", "Id", cascadeDelete: true);
            AddForeignKey("dbo.LocalGoalTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.LocalGoalTypes", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItSystemUsageOverviewItProjectReadModels", "ParentId", "dbo.ItSystemUsageOverviewReadModels", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UserNotifications", "ItProject_Id", "dbo.ItProject", "Id");
            AddForeignKey("dbo.ItProjectOrgUnitUsages", "ItProjectId", "dbo.ItProject", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItProjectTaskRefs", "TaskRef_Id", "dbo.TaskRef", "Id");
            AddForeignKey("dbo.ItProjectTaskRefs", "ItProject_Id", "dbo.ItProject", "Id");
            AddForeignKey("dbo.Stakeholder", "ItProjectId", "dbo.ItProject", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Stakeholder", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.Stakeholder", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.Risk", "ItProjectId", "dbo.ItProject", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Risk", "ResponsibleUserId", "dbo.User", "Id");
            AddForeignKey("dbo.Risk", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.Risk", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItProjectRights", "UserId", "dbo.User", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItProjectRights", "RoleId", "dbo.ItProjectRoles", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItProjectRoles", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.ItProjectRoles", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.AdviceUserRelations", "ItProjectRoleId", "dbo.ItProjectRoles", "Id");
            AddForeignKey("dbo.ItProjectRights", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.ItProjectRights", "ObjectId", "dbo.ItProject", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItProjectRights", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItProjectOrgUnitUsages", "ResponsibleItProject_Id", "dbo.ItProject", "Id");
            AddForeignKey("dbo.ItProjectOrgUnitUsages", "OrganizationUnitId", "dbo.OrganizationUnit", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItProject", "ReferenceId", "dbo.ExternalReferences", "Id");
            AddForeignKey("dbo.ItProject", "ParentId", "dbo.ItProject", "Id");
            AddForeignKey("dbo.ItProject", "OrganizationId", "dbo.Organization", "Id");
            AddForeignKey("dbo.ItProject", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.ItProject", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItProject", "JointMunicipalProjectId", "dbo.ItProject", "Id");
            AddForeignKey("dbo.ItProjectItSystemUsages", "ItSystemUsage_Id", "dbo.ItSystemUsage", "Id");
            AddForeignKey("dbo.ItProjectItSystemUsages", "ItProject_Id", "dbo.ItProject", "Id");
            AddForeignKey("dbo.ItProject", "ItProjectTypeId", "dbo.ItProjectTypes", "Id");
            AddForeignKey("dbo.ItProjectTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.ItProjectTypes", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItProjectStatusUpdates", "AssociatedItProjectId", "dbo.ItProject", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItProjectStatusUpdates", "OrganizationId", "dbo.Organization", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItProjectStatusUpdates", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.ItProjectStatusUpdates", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItProjectStatus", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.ItProjectStatus", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItProjectStatus", "AssociatedUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItProjectStatus", "AssociatedItProjectId", "dbo.ItProject", "Id", cascadeDelete: true);
            AddForeignKey("dbo.HandoverUsers", "User_Id", "dbo.User", "Id");
            AddForeignKey("dbo.HandoverUsers", "Handover_Id", "dbo.Handover", "Id");
            AddForeignKey("dbo.Handover", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.Handover", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.Handover", "Id", "dbo.ItProject", "Id", cascadeDelete: true);
            AddForeignKey("dbo.GoalStatus", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.GoalStatus", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.GoalStatus", "Id", "dbo.ItProject", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Goal", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.Goal", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.Goal", "GoalTypeId", "dbo.GoalTypes", "Id");
            AddForeignKey("dbo.GoalTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.GoalTypes", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.Goal", "GoalStatusId", "dbo.GoalStatus", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ExternalReferences", "ItProject_Id", "dbo.ItProject", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EconomyYears", "ItProjectId", "dbo.ItProject", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EconomyYears", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.EconomyYears", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.Communication", "ResponsibleUserId", "dbo.User", "Id");
            AddForeignKey("dbo.Communication", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.Communication", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.Communication", "ItProjectId", "dbo.ItProject", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItProject", "CommonPublicProjectId", "dbo.ItProject", "Id");
        }
    }
}
