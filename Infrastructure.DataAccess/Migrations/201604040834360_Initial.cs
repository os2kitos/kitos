namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "AdminRights",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        ObjectId = c.Int(nullable: false),
                        DefaultOrgUnitId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("OrganizationUnit", t => t.DefaultOrgUnitId)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("Organization", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("AdminRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.DefaultOrgUnitId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "OrganizationUnit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LocalId = c.String(maxLength: 100, storeType: "nvarchar"),
                        Name = c.String(unicode: false),
                        Ean = c.Long(),
                        ParentId = c.Int(),
                        OrganizationId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("OrganizationUnit", t => t.ParentId, cascadeDelete: true)
                .Index(t => new { t.OrganizationId, t.LocalId }, unique: true, name: "UniqueLocalId")
                .Index(t => t.ParentId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItSystemUsage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsStatusActive = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        LocalSystemId = c.String(unicode: false),
                        Version = c.String(unicode: false),
                        EsdhRef = c.String(unicode: false),
                        CmdbRef = c.String(unicode: false),
                        DirectoryOrUrlRef = c.String(unicode: false),
                        LocalCallName = c.String(unicode: false),
                        OrganizationId = c.Int(nullable: false),
                        ItSystemId = c.Int(nullable: false),
                        ArchiveTypeId = c.Int(),
                        SensitiveDataTypeId = c.Int(),
                        OverviewId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                        OrganizationUnit_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ArchiveTypes", t => t.ArchiveTypeId)
                .ForeignKey("ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("ItSystemUsage", t => t.OverviewId)
                .ForeignKey("SensitiveDataTypes", t => t.SensitiveDataTypeId)
                .ForeignKey("OrganizationUnit", t => t.OrganizationUnit_Id)
                .Index(t => t.OrganizationId)
                .Index(t => t.ItSystemId)
                .Index(t => t.ArchiveTypeId)
                .Index(t => t.SensitiveDataTypeId)
                .Index(t => t.OverviewId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.OrganizationUnit_Id);

            CreateTable(
                "ArchiveTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "User",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        LastName = c.String(unicode: false),
                        PhoneNumber = c.String(unicode: false),
                        Email = c.String(nullable: false, maxLength: 100, storeType: "nvarchar"),
                        Password = c.String(nullable: false, unicode: false),
                        Salt = c.String(nullable: false, unicode: false),
                        IsGlobalAdmin = c.Boolean(nullable: false),
                        Uuid = c.Guid(),
                        LastAdvisDate = c.DateTime(precision: 0),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId)
                .ForeignKey("User", t => t.ObjectOwnerId)
                .Index(t => t.Email, unique: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "Handover",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Description = c.String(unicode: false),
                        MeetingDate = c.DateTime(precision: 0),
                        Summary = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ItProject", t => t.Id, cascadeDelete: true)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItProject",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.String(unicode: false),
                        Background = c.String(unicode: false),
                        Note = c.String(unicode: false),
                        Name = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        AccessModifier = c.Int(nullable: false),
                        IsArchived = c.Boolean(nullable: false),
                        Esdh = c.String(unicode: false),
                        Cmdb = c.String(unicode: false),
                        Folder = c.String(unicode: false),
                        ParentId = c.Int(),
                        ItProjectTypeId = c.Int(),
                        OrganizationId = c.Int(nullable: false),
                        Priority = c.Int(nullable: false),
                        IsPriorityLocked = c.Boolean(nullable: false),
                        PriorityPf = c.Int(nullable: false),
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
                        StatusProject = c.Int(nullable: false),
                        StatusDate = c.DateTime(precision: 0),
                        StatusNote = c.String(unicode: false),
                        Phase1_Name = c.String(unicode: false),
                        Phase1_StartDate = c.DateTime(precision: 0),
                        Phase1_EndDate = c.DateTime(precision: 0),
                        Phase2_Name = c.String(unicode: false),
                        Phase2_StartDate = c.DateTime(precision: 0),
                        Phase2_EndDate = c.DateTime(precision: 0),
                        Phase3_Name = c.String(unicode: false),
                        Phase3_StartDate = c.DateTime(precision: 0),
                        Phase3_EndDate = c.DateTime(precision: 0),
                        Phase4_Name = c.String(unicode: false),
                        Phase4_StartDate = c.DateTime(precision: 0),
                        Phase4_EndDate = c.DateTime(precision: 0),
                        Phase5_Name = c.String(unicode: false),
                        Phase5_StartDate = c.DateTime(precision: 0),
                        Phase5_EndDate = c.DateTime(precision: 0),
                        CurrentPhase = c.Int(nullable: false),
                        OriginalId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ItProject", t => t.CommonPublicProjectId)
                .ForeignKey("ItProjectTypes", t => t.ItProjectTypeId)
                .ForeignKey("ItProject", t => t.JointMunicipalProjectId)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("Organization", t => t.OrganizationId)
                .ForeignKey("ItProject", t => t.OriginalId)
                .ForeignKey("ItProject", t => t.ParentId)
                .Index(t => t.ParentId)
                .Index(t => t.ItProjectTypeId)
                .Index(t => t.OrganizationId)
                .Index(t => t.JointMunicipalProjectId)
                .Index(t => t.CommonPublicProjectId)
                .Index(t => t.OriginalId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "Communication",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TargetAudiance = c.String(unicode: false),
                        Purpose = c.String(unicode: false),
                        Message = c.String(unicode: false),
                        Media = c.String(unicode: false),
                        DueDate = c.DateTime(precision: 0),
                        ResponsibleUserId = c.Int(),
                        ItProjectId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ItProject", t => t.ItProjectId, cascadeDelete: true)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("User", t => t.ResponsibleUserId)
                .Index(t => t.ResponsibleUserId)
                .Index(t => t.ItProjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "EconomyYears",
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
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "GoalStatus",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        StatusDate = c.DateTime(precision: 0),
                        StatusNote = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ItProject", t => t.Id, cascadeDelete: true)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "Goal",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HumanReadableId = c.String(unicode: false),
                        Name = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        Note = c.String(unicode: false),
                        Measurable = c.Boolean(nullable: false),
                        Status = c.Int(nullable: false),
                        GoalTypeId = c.Int(),
                        GoalStatusId = c.Int(nullable: false),
                        SubGoalDate1 = c.DateTime(precision: 0),
                        SubGoalDate2 = c.DateTime(precision: 0),
                        SubGoalDate3 = c.DateTime(precision: 0),
                        SubGoal1 = c.String(unicode: false),
                        SubGoal2 = c.String(unicode: false),
                        SubGoal3 = c.String(unicode: false),
                        SubGoalRea1 = c.String(unicode: false),
                        SubGoalRea2 = c.String(unicode: false),
                        SubGoalRea3 = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("GoalStatus", t => t.GoalStatusId, cascadeDelete: true)
                .ForeignKey("GoalTypes", t => t.GoalTypeId)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.GoalTypeId)
                .Index(t => t.GoalStatusId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "GoalTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItProjectStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HumanReadableId = c.String(unicode: false),
                        Name = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        Note = c.String(unicode: false),
                        TimeEstimate = c.Int(nullable: false),
                        AssociatedUserId = c.Int(),
                        AssociatedItProjectId = c.Int(nullable: false),
                        AssociatedPhaseNum = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                        StartDate = c.DateTime(precision: 0),
                        EndDate = c.DateTime(precision: 0),
                        StatusProcentage = c.Int(),
                        Date = c.DateTime(precision: 0),
                        Status = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ItProject", t => t.AssociatedItProjectId, cascadeDelete: true)
                .ForeignKey("User", t => t.AssociatedUserId)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.AssociatedUserId)
                .Index(t => t.AssociatedItProjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItProjectTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "Organization",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100, storeType: "nvarchar"),
                        Type = c.Int(nullable: false),
                        Cvr = c.String(maxLength: 10, storeType: "nvarchar"),
                        AccessModifier = c.Int(nullable: false),
                        Uuid = c.Guid(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.Name)
                .Index(t => t.Cvr, unique: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItSystem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemId = c.String(unicode: false),
                        AppTypeOptionId = c.Int(),
                        ParentId = c.Int(),
                        BusinessTypeId = c.Int(),
                        Name = c.String(nullable: false, maxLength: 100, storeType: "nvarchar"),
                        Uuid = c.Guid(nullable: false),
                        Description = c.String(unicode: false),
                        Url = c.String(unicode: false),
                        AccessModifier = c.Int(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        BelongsToId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ItSystemTypeOptions", t => t.AppTypeOptionId)
                .ForeignKey("Organization", t => t.BelongsToId)
                .ForeignKey("BusinessTypes", t => t.BusinessTypeId)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("ItSystem", t => t.ParentId)
                .Index(t => t.AppTypeOptionId)
                .Index(t => t.ParentId)
                .Index(t => t.BusinessTypeId)
                .Index(t => new { t.OrganizationId, t.Name }, unique: true, name: "IX_NamePerOrg")
                .Index(t => t.BelongsToId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItSystemTypeOptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "BusinessTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItInterfaceUses",
                c => new
                    {
                        ItSystemId = c.Int(nullable: false),
                        ItInterfaceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItSystemId, t.ItInterfaceId })
                .ForeignKey("ItInterface", t => t.ItInterfaceId, cascadeDelete: true)
                .ForeignKey("ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .Index(t => t.ItSystemId)
                .Index(t => t.ItInterfaceId);

            CreateTable(
                "InfUsage",
                c => new
                    {
                        ItSystemUsageId = c.Int(nullable: false),
                        ItSystemId = c.Int(nullable: false),
                        ItInterfaceId = c.Int(nullable: false),
                        ItContractId = c.Int(),
                        InfrastructureId = c.Int(),
                        IsWishedFor = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItSystemUsageId, t.ItSystemId, t.ItInterfaceId })
                .ForeignKey("ItInterface", t => t.ItInterfaceId, cascadeDelete: true)
                .ForeignKey("ItSystemUsage", t => t.InfrastructureId)
                .ForeignKey("ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("ItInterfaceUses", t => new { t.ItSystemId, t.ItInterfaceId }, cascadeDelete: true)
                .ForeignKey("ItSystemUsage", t => t.ItSystemUsageId, cascadeDelete: true)
                .Index(t => t.ItSystemUsageId)
                .Index(t => new { t.ItSystemId, t.ItInterfaceId })
                .Index(t => t.ItInterfaceId)
                .Index(t => t.ItContractId)
                .Index(t => t.InfrastructureId);

            CreateTable(
                "DataRowUsage",
                c => new
                    {
                        DataRowId = c.Int(nullable: false),
                        SysUsageId = c.Int(nullable: false),
                        SysId = c.Int(nullable: false),
                        IntfId = c.Int(nullable: false),
                        FrequencyId = c.Int(),
                        Amount = c.Int(),
                        Economy = c.Int(),
                        Price = c.Int(),
                    })
                .PrimaryKey(t => new { t.DataRowId, t.SysUsageId, t.SysId, t.IntfId })
                .ForeignKey("DataRow", t => t.DataRowId, cascadeDelete: true)
                .ForeignKey("Frequencies", t => t.FrequencyId)
                .ForeignKey("InfUsage", t => new { t.SysUsageId, t.SysId, t.IntfId }, cascadeDelete: true)
                .Index(t => t.DataRowId)
                .Index(t => new { t.SysUsageId, t.SysId, t.IntfId })
                .Index(t => t.FrequencyId);

            CreateTable(
                "DataRow",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItInterfaceId = c.Int(nullable: false),
                        DataTypeId = c.Int(),
                        Data = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("DataTypes", t => t.DataTypeId)
                .ForeignKey("ItInterface", t => t.ItInterfaceId, cascadeDelete: true)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ItInterfaceId)
                .Index(t => t.DataTypeId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "DataTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItInterface",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Version = c.String(maxLength: 20, storeType: "nvarchar"),
                        ItInterfaceId = c.String(nullable: false, maxLength: 100, storeType: "nvarchar"),
                        InterfaceId = c.Int(),
                        InterfaceTypeId = c.Int(),
                        TsaId = c.Int(),
                        MethodId = c.Int(),
                        Note = c.String(unicode: false),
                        Name = c.String(nullable: false, maxLength: 100, storeType: "nvarchar"),
                        Uuid = c.Guid(nullable: false),
                        Description = c.String(unicode: false),
                        Url = c.String(unicode: false),
                        AccessModifier = c.Int(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        BelongsToId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("Organization", t => t.BelongsToId)
                .ForeignKey("Interfaces", t => t.InterfaceId)
                .ForeignKey("InterfaceTypes", t => t.InterfaceTypeId)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("Methods", t => t.MethodId)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("Tsas", t => t.TsaId)
                .Index(t => new { t.OrganizationId, t.Name, t.ItInterfaceId }, unique: true, name: "IX_NamePerOrg")
                .Index(t => t.InterfaceId)
                .Index(t => t.InterfaceTypeId)
                .Index(t => t.TsaId)
                .Index(t => t.MethodId)
                .Index(t => t.BelongsToId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "Exhibit",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ItSystemId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ItInterface", t => t.Id, cascadeDelete: true)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.ItSystemId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItInterfaceExhibitUsage",
                c => new
                    {
                        ItSystemUsageId = c.Int(nullable: false),
                        ItInterfaceExhibitId = c.Int(nullable: false),
                        ItContractId = c.Int(),
                        IsWishedFor = c.Boolean(nullable: false),
                        ItInterface_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItSystemUsageId, t.ItInterfaceExhibitId })
                .ForeignKey("ItContract", t => t.ItContractId)
                .ForeignKey("Exhibit", t => t.ItInterfaceExhibitId, cascadeDelete: true)
                .ForeignKey("ItSystemUsage", t => t.ItSystemUsageId, cascadeDelete: true)
                .ForeignKey("ItInterface", t => t.ItInterface_Id)
                .Index(t => t.ItSystemUsageId)
                .Index(t => t.ItInterfaceExhibitId)
                .Index(t => t.ItContractId)
                .Index(t => t.ItInterface_Id);

            CreateTable(
                "ItContract",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        Note = c.String(unicode: false),
                        ItContractId = c.String(unicode: false),
                        Esdh = c.String(unicode: false),
                        Folder = c.String(unicode: false),
                        SupplierContractSigner = c.String(unicode: false),
                        HasSupplierSigned = c.Boolean(nullable: false),
                        SupplierSignedDate = c.DateTime(precision: 0),
                        ContractSignerId = c.Int(),
                        IsSigned = c.Boolean(nullable: false),
                        SignedDate = c.DateTime(precision: 0),
                        ResponsibleOrganizationUnitId = c.Int(),
                        OrganizationId = c.Int(nullable: false),
                        SupplierId = c.Int(),
                        ProcurementStrategyId = c.Int(),
                        ProcurementPlanHalf = c.Int(),
                        ProcurementPlanYear = c.Int(),
                        ContractTemplateId = c.Int(),
                        ContractTypeId = c.Int(),
                        PurchaseFormId = c.Int(),
                        ParentId = c.Int(),
                        OperationTestExpected = c.DateTime(precision: 0),
                        OperationTestApproved = c.DateTime(precision: 0),
                        OperationalAcceptanceTestExpected = c.DateTime(precision: 0),
                        OperationalAcceptanceTestApproved = c.DateTime(precision: 0),
                        Concluded = c.DateTime(precision: 0),
                        Duration = c.Int(),
                        IrrevocableTo = c.DateTime(precision: 0),
                        ExpirationDate = c.DateTime(precision: 0),
                        Terminated = c.DateTime(precision: 0),
                        TerminationDeadlineId = c.Int(),
                        OptionExtendId = c.Int(),
                        ExtendMultiplier = c.Int(nullable: false),
                        OperationRemunerationBegun = c.DateTime(precision: 0),
                        PaymentFreqencyId = c.Int(),
                        PaymentModelId = c.Int(),
                        PriceRegulationId = c.Int(),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.ContractSignerId)
                .ForeignKey("ContractTemplates", t => t.ContractTemplateId)
                .ForeignKey("ContractTypes", t => t.ContractTypeId)
                .ForeignKey("User", t => t.LastChangedByUserId)
                .ForeignKey("User", t => t.ObjectOwnerId)
                .ForeignKey("OptionExtends", t => t.OptionExtendId)
                .ForeignKey("Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("ItContract", t => t.ParentId)
                .ForeignKey("PaymentFreqencies", t => t.PaymentFreqencyId)
                .ForeignKey("PaymentModels", t => t.PaymentModelId)
                .ForeignKey("PriceRegulations", t => t.PriceRegulationId)
                .ForeignKey("ProcurementStrategies", t => t.ProcurementStrategyId)
                .ForeignKey("PurchaseForms", t => t.PurchaseFormId)
                .ForeignKey("OrganizationUnit", t => t.ResponsibleOrganizationUnitId)
                .ForeignKey("Organization", t => t.SupplierId)
                .ForeignKey("TerminationDeadlines", t => t.TerminationDeadlineId)
                .Index(t => t.ContractSignerId)
                .Index(t => t.ResponsibleOrganizationUnitId)
                .Index(t => t.OrganizationId)
                .Index(t => t.SupplierId)
                .Index(t => t.ProcurementStrategyId)
                .Index(t => t.ContractTemplateId)
                .Index(t => t.ContractTypeId)
                .Index(t => t.PurchaseFormId)
                .Index(t => t.ParentId)
                .Index(t => t.TerminationDeadlineId)
                .Index(t => t.OptionExtendId)
                .Index(t => t.PaymentFreqencyId)
                .Index(t => t.PaymentModelId)
                .Index(t => t.PriceRegulationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "Advice",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsActive = c.Boolean(nullable: false),
                        Name = c.String(unicode: false),
                        AlarmDate = c.DateTime(precision: 0),
                        SentDate = c.DateTime(precision: 0),
                        ReceiverId = c.Int(),
                        CarbonCopyReceiverId = c.Int(),
                        Subject = c.String(unicode: false),
                        ItContractId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ItContractRoles", t => t.CarbonCopyReceiverId)
                .ForeignKey("ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("ItContractRoles", t => t.ReceiverId)
                .Index(t => t.ReceiverId)
                .Index(t => t.CarbonCopyReceiverId)
                .Index(t => t.ItContractId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItContractRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItContractRights",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        ObjectId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("ItContract", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("ItContractRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "AgreementElements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItContractItSystemUsages",
                c => new
                    {
                        ItContractId = c.Int(nullable: false),
                        ItSystemUsageId = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItContractId, t.ItSystemUsageId })
                .ForeignKey("ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("ItSystemUsage", t => t.ItSystemUsageId, cascadeDelete: true)
                .ForeignKey("ItSystemUsage", t => t.ItSystemUsage_Id)
                .Index(t => t.ItContractId)
                .Index(t => t.ItSystemUsageId)
                .Index(t => t.ItSystemUsage_Id);

            CreateTable(
                "ContractTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ContractTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "EconomyStream",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExternPaymentForId = c.Int(),
                        InternPaymentForId = c.Int(),
                        OrganizationUnitId = c.Int(),
                        Acquisition = c.Int(nullable: false),
                        Operation = c.Int(nullable: false),
                        Other = c.Int(nullable: false),
                        AccountingEntry = c.String(unicode: false),
                        AuditStatus = c.Int(nullable: false),
                        AuditDate = c.DateTime(precision: 0),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ItContract", t => t.ExternPaymentForId, cascadeDelete: true)
                .ForeignKey("ItContract", t => t.InternPaymentForId, cascadeDelete: true)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("OrganizationUnit", t => t.OrganizationUnitId)
                .Index(t => t.ExternPaymentForId)
                .Index(t => t.InternPaymentForId)
                .Index(t => t.OrganizationUnitId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "HandoverTrial",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Expected = c.DateTime(precision: 0),
                        Approved = c.DateTime(precision: 0),
                        ItContractId = c.Int(nullable: false),
                        HandoverTrialTypeId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("HandoverTrialTypes", t => t.HandoverTrialTypeId)
                .ForeignKey("ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ItContractId)
                .Index(t => t.HandoverTrialTypeId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "HandoverTrialTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "OptionExtends",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId)
                .ForeignKey("User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "PaymentFreqencies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId)
                .ForeignKey("User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "PaymentMilestones",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(unicode: false),
                        Expected = c.DateTime(precision: 0),
                        Approved = c.DateTime(precision: 0),
                        ItContractId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("User", t => t.LastChangedByUserId)
                .ForeignKey("User", t => t.ObjectOwnerId)
                .Index(t => t.ItContractId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "PaymentModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId)
                .ForeignKey("User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "PriceRegulations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId)
                .ForeignKey("User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ProcurementStrategies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "PurchaseForms",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "TerminationDeadlines",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId)
                .ForeignKey("User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "Interfaces",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "InterfaceTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "Methods",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "Tsas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "Frequencies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "TaskRef",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccessModifier = c.Int(nullable: false),
                        Uuid = c.Guid(nullable: false),
                        Type = c.String(unicode: false),
                        TaskKey = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        ActiveFrom = c.DateTime(precision: 0),
                        ActiveTo = c.DateTime(precision: 0),
                        ParentId = c.Int(),
                        OwnedByOrganizationUnitId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                        OrganizationUnit_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("OrganizationUnit", t => t.OwnedByOrganizationUnitId, cascadeDelete: true)
                .ForeignKey("TaskRef", t => t.ParentId)
                .ForeignKey("OrganizationUnit", t => t.OrganizationUnit_Id)
                .Index(t => t.ParentId)
                .Index(t => t.OwnedByOrganizationUnitId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.OrganizationUnit_Id);

            CreateTable(
                "TaskUsage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TaskRefId = c.Int(nullable: false),
                        OrgUnitId = c.Int(nullable: false),
                        ParentId = c.Int(),
                        Starred = c.Boolean(nullable: false),
                        TechnologyStatus = c.Int(nullable: false),
                        UsageStatus = c.Int(nullable: false),
                        Comment = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("OrganizationUnit", t => t.OrgUnitId, cascadeDelete: true)
                .ForeignKey("TaskUsage", t => t.ParentId, cascadeDelete: true)
                .ForeignKey("TaskRef", t => t.TaskRefId, cascadeDelete: true)
                .Index(t => t.TaskRefId)
                .Index(t => t.OrgUnitId)
                .Index(t => t.ParentId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "Wish",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsPublic = c.Boolean(nullable: false),
                        Text = c.String(unicode: false),
                        UserId = c.Int(nullable: false),
                        ItSystemUsageId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                        ItSystem_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ItSystemUsage", t => t.ItSystemUsageId, cascadeDelete: true)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("User", t => t.UserId, cascadeDelete: true)
                .ForeignKey("ItSystem", t => t.ItSystem_Id)
                .Index(t => t.UserId)
                .Index(t => t.ItSystemUsageId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.ItSystem_Id);

            CreateTable(
                "Config",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ShowItProjectModule = c.Boolean(nullable: false),
                        ShowItSystemModule = c.Boolean(nullable: false),
                        ShowItContractModule = c.Boolean(nullable: false),
                        ItSupportModuleNameId = c.Int(nullable: false),
                        ItSupportGuide = c.String(unicode: false),
                        ShowTabOverview = c.Boolean(nullable: false),
                        ShowColumnTechnology = c.Boolean(nullable: false),
                        ShowColumnUsage = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("Organization", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItProjectOrgUnitUsages",
                c => new
                    {
                        ItProjectId = c.Int(nullable: false),
                        OrganizationUnitId = c.Int(nullable: false),
                        ResponsibleItProject_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItProjectId, t.OrganizationUnitId })
                .ForeignKey("ItProject", t => t.ResponsibleItProject_Id)
                .ForeignKey("ItProject", t => t.ItProjectId, cascadeDelete: true)
                .ForeignKey("OrganizationUnit", t => t.OrganizationUnitId, cascadeDelete: true)
                .Index(t => t.ItProjectId)
                .Index(t => t.OrganizationUnitId)
                .Index(t => t.ResponsibleItProject_Id);

            CreateTable(
                "ItProjectRights",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        ObjectId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("ItProject", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("ItProjectRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItProjectRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "Risk",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.Int(nullable: false),
                        Name = c.String(unicode: false),
                        Action = c.String(unicode: false),
                        Probability = c.Int(nullable: false),
                        Consequence = c.Int(nullable: false),
                        ResponsibleUserId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("User", t => t.ResponsibleUserId)
                .ForeignKey("ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId)
                .Index(t => t.ResponsibleUserId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "Stakeholder",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.Int(nullable: false),
                        Name = c.String(unicode: false),
                        Role = c.String(unicode: false),
                        Downsides = c.String(unicode: false),
                        Benefits = c.String(unicode: false),
                        Significance = c.Int(nullable: false),
                        HowToHandle = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "PasswordResetRequest",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Hash = c.String(unicode: false),
                        Time = c.DateTime(nullable: false, precision: 0),
                        UserId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "itusageorgusage",
                c => new
                    {
                        ItSystemUsageId = c.Int(nullable: false),
                        OrganizationUnitId = c.Int(nullable: false),
                        ResponsibleItSystemUsage_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItSystemUsageId, t.OrganizationUnitId })
                .ForeignKey("ItSystemUsage", t => t.ResponsibleItSystemUsage_Id)
                .ForeignKey("ItSystemUsage", t => t.ItSystemUsageId, cascadeDelete: true)
                .ForeignKey("OrganizationUnit", t => t.OrganizationUnitId, cascadeDelete: true)
                .Index(t => t.ItSystemUsageId)
                .Index(t => t.OrganizationUnitId)
                .Index(t => t.ResponsibleItSystemUsage_Id);

            CreateTable(
                "ItSystemRights",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        ObjectId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("ItSystemUsage", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("ItSystemRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItSystemRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "SensitiveDataTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "OrganizationRights",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        ObjectId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("OrganizationUnit", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("OrganizationRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "OrganizationRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "AdminRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "Text",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "ItProjectItSystemUsages",
                c => new
                    {
                        ItProject_Id = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItProject_Id, t.ItSystemUsage_Id })
                .ForeignKey("ItProject", t => t.ItProject_Id, cascadeDelete: true)
                .ForeignKey("ItSystemUsage", t => t.ItSystemUsage_Id, cascadeDelete: true)
                .Index(t => t.ItProject_Id)
                .Index(t => t.ItSystemUsage_Id);

            CreateTable(
                "ItContractAgreementElements",
                c => new
                    {
                        ItContractId = c.Int(nullable: false),
                        ElemId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItContractId, t.ElemId })
                .ForeignKey("ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("AgreementElements", t => t.ElemId, cascadeDelete: true)
                .Index(t => t.ItContractId)
                .Index(t => t.ElemId);

            CreateTable(
                "TaskRefItSystems",
                c => new
                    {
                        TaskRef_Id = c.Int(nullable: false),
                        ItSystem_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TaskRef_Id, t.ItSystem_Id })
                .ForeignKey("TaskRef", t => t.TaskRef_Id, cascadeDelete: true)
                .ForeignKey("ItSystem", t => t.ItSystem_Id, cascadeDelete: true)
                .Index(t => t.TaskRef_Id)
                .Index(t => t.ItSystem_Id);

            CreateTable(
                "TaskRefItSystemUsages",
                c => new
                    {
                        TaskRef_Id = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TaskRef_Id, t.ItSystemUsage_Id })
                .ForeignKey("TaskRef", t => t.TaskRef_Id, cascadeDelete: true)
                .ForeignKey("ItSystemUsage", t => t.ItSystemUsage_Id, cascadeDelete: true)
                .Index(t => t.TaskRef_Id)
                .Index(t => t.ItSystemUsage_Id);

            CreateTable(
                "ItProjectTaskRefs",
                c => new
                    {
                        ItProject_Id = c.Int(nullable: false),
                        TaskRef_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItProject_Id, t.TaskRef_Id })
                .ForeignKey("ItProject", t => t.ItProject_Id, cascadeDelete: true)
                .ForeignKey("TaskRef", t => t.TaskRef_Id, cascadeDelete: true)
                .Index(t => t.ItProject_Id)
                .Index(t => t.TaskRef_Id);

            CreateTable(
                "HandoverUsers",
                c => new
                    {
                        Handover_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Handover_Id, t.User_Id })
                .ForeignKey("Handover", t => t.Handover_Id, cascadeDelete: true)
                .ForeignKey("User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Handover_Id)
                .Index(t => t.User_Id);

            CreateTable(
                "OrgUnitSystemUsage",
                c => new
                    {
                        ItSystemUsage_Id = c.Int(nullable: false),
                        OrganizationUnit_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItSystemUsage_Id, t.OrganizationUnit_Id })
                .ForeignKey("ItSystemUsage", t => t.ItSystemUsage_Id, cascadeDelete: true)
                .ForeignKey("OrganizationUnit", t => t.OrganizationUnit_Id, cascadeDelete: true)
                .Index(t => t.ItSystemUsage_Id)
                .Index(t => t.OrganizationUnit_Id);

        }

        public override void Down()
        {
            DropForeignKey("Text", "ObjectOwnerId", "User");
            DropForeignKey("Text", "LastChangedByUserId", "User");
            DropForeignKey("AdminRights", "UserId", "User");
            DropForeignKey("AdminRights", "RoleId", "AdminRoles");
            DropForeignKey("AdminRoles", "ObjectOwnerId", "User");
            DropForeignKey("AdminRoles", "LastChangedByUserId", "User");
            DropForeignKey("AdminRights", "ObjectOwnerId", "User");
            DropForeignKey("AdminRights", "ObjectId", "Organization");
            DropForeignKey("AdminRights", "LastChangedByUserId", "User");
            DropForeignKey("AdminRights", "DefaultOrgUnitId", "OrganizationUnit");
            DropForeignKey("ItProjectOrgUnitUsages", "OrganizationUnitId", "OrganizationUnit");
            DropForeignKey("itusageorgusage", "OrganizationUnitId", "OrganizationUnit");
            DropForeignKey("TaskRef", "OrganizationUnit_Id", "OrganizationUnit");
            DropForeignKey("OrganizationRights", "UserId", "User");
            DropForeignKey("OrganizationRights", "RoleId", "OrganizationRoles");
            DropForeignKey("OrganizationRoles", "ObjectOwnerId", "User");
            DropForeignKey("OrganizationRoles", "LastChangedByUserId", "User");
            DropForeignKey("OrganizationRights", "ObjectOwnerId", "User");
            DropForeignKey("OrganizationRights", "ObjectId", "OrganizationUnit");
            DropForeignKey("OrganizationRights", "LastChangedByUserId", "User");
            DropForeignKey("OrganizationUnit", "ParentId", "OrganizationUnit");
            DropForeignKey("OrganizationUnit", "OrganizationId", "Organization");
            DropForeignKey("OrganizationUnit", "ObjectOwnerId", "User");
            DropForeignKey("OrganizationUnit", "LastChangedByUserId", "User");
            DropForeignKey("ItSystemUsage", "OrganizationUnit_Id", "OrganizationUnit");
            DropForeignKey("itusageorgusage", "ItSystemUsageId", "ItSystemUsage");
            DropForeignKey("ItSystemUsage", "SensitiveDataTypeId", "SensitiveDataTypes");
            DropForeignKey("SensitiveDataTypes", "ObjectOwnerId", "User");
            DropForeignKey("SensitiveDataTypes", "LastChangedByUserId", "User");
            DropForeignKey("ItSystemRights", "UserId", "User");
            DropForeignKey("ItSystemRights", "RoleId", "ItSystemRoles");
            DropForeignKey("ItSystemRoles", "ObjectOwnerId", "User");
            DropForeignKey("ItSystemRoles", "LastChangedByUserId", "User");
            DropForeignKey("ItSystemRights", "ObjectOwnerId", "User");
            DropForeignKey("ItSystemRights", "ObjectId", "ItSystemUsage");
            DropForeignKey("ItSystemRights", "LastChangedByUserId", "User");
            DropForeignKey("itusageorgusage", "ResponsibleItSystemUsage_Id", "ItSystemUsage");
            DropForeignKey("ItSystemUsage", "OverviewId", "ItSystemUsage");
            DropForeignKey("OrgUnitSystemUsage", "OrganizationUnit_Id", "OrganizationUnit");
            DropForeignKey("OrgUnitSystemUsage", "ItSystemUsage_Id", "ItSystemUsage");
            DropForeignKey("ItSystemUsage", "OrganizationId", "Organization");
            DropForeignKey("ItSystemUsage", "ObjectOwnerId", "User");
            DropForeignKey("ItContractItSystemUsages", "ItSystemUsage_Id", "ItSystemUsage");
            DropForeignKey("ItSystemUsage", "LastChangedByUserId", "User");
            DropForeignKey("ItSystemUsage", "ItSystemId", "ItSystem");
            DropForeignKey("ItContractItSystemUsages", "ItSystemUsageId", "ItSystemUsage");
            DropForeignKey("ItSystemUsage", "ArchiveTypeId", "ArchiveTypes");
            DropForeignKey("ArchiveTypes", "ObjectOwnerId", "User");
            DropForeignKey("ArchiveTypes", "LastChangedByUserId", "User");
            DropForeignKey("PasswordResetRequest", "UserId", "User");
            DropForeignKey("PasswordResetRequest", "ObjectOwnerId", "User");
            DropForeignKey("PasswordResetRequest", "LastChangedByUserId", "User");
            DropForeignKey("User", "ObjectOwnerId", "User");
            DropForeignKey("User", "LastChangedByUserId", "User");
            DropForeignKey("HandoverUsers", "User_Id", "User");
            DropForeignKey("HandoverUsers", "Handover_Id", "Handover");
            DropForeignKey("Handover", "ObjectOwnerId", "User");
            DropForeignKey("Handover", "LastChangedByUserId", "User");
            DropForeignKey("Handover", "Id", "ItProject");
            DropForeignKey("ItProjectOrgUnitUsages", "ItProjectId", "ItProject");
            DropForeignKey("ItProjectTaskRefs", "TaskRef_Id", "TaskRef");
            DropForeignKey("ItProjectTaskRefs", "ItProject_Id", "ItProject");
            DropForeignKey("Stakeholder", "ItProjectId", "ItProject");
            DropForeignKey("Stakeholder", "ObjectOwnerId", "User");
            DropForeignKey("Stakeholder", "LastChangedByUserId", "User");
            DropForeignKey("Risk", "ItProjectId", "ItProject");
            DropForeignKey("Risk", "ResponsibleUserId", "User");
            DropForeignKey("Risk", "ObjectOwnerId", "User");
            DropForeignKey("Risk", "LastChangedByUserId", "User");
            DropForeignKey("ItProjectRights", "UserId", "User");
            DropForeignKey("ItProjectRights", "RoleId", "ItProjectRoles");
            DropForeignKey("ItProjectRoles", "ObjectOwnerId", "User");
            DropForeignKey("ItProjectRoles", "LastChangedByUserId", "User");
            DropForeignKey("ItProjectRights", "ObjectOwnerId", "User");
            DropForeignKey("ItProjectRights", "ObjectId", "ItProject");
            DropForeignKey("ItProjectRights", "LastChangedByUserId", "User");
            DropForeignKey("ItProjectOrgUnitUsages", "ResponsibleItProject_Id", "ItProject");
            DropForeignKey("ItProject", "ParentId", "ItProject");
            DropForeignKey("ItProject", "OriginalId", "ItProject");
            DropForeignKey("ItProject", "OrganizationId", "Organization");
            DropForeignKey("Organization", "ObjectOwnerId", "User");
            DropForeignKey("Organization", "LastChangedByUserId", "User");
            DropForeignKey("Config", "Id", "Organization");
            DropForeignKey("Config", "ObjectOwnerId", "User");
            DropForeignKey("Config", "LastChangedByUserId", "User");
            DropForeignKey("Wish", "ItSystem_Id", "ItSystem");
            DropForeignKey("Wish", "UserId", "User");
            DropForeignKey("Wish", "ObjectOwnerId", "User");
            DropForeignKey("Wish", "LastChangedByUserId", "User");
            DropForeignKey("Wish", "ItSystemUsageId", "ItSystemUsage");
            DropForeignKey("TaskUsage", "TaskRefId", "TaskRef");
            DropForeignKey("TaskUsage", "ParentId", "TaskUsage");
            DropForeignKey("TaskUsage", "OrgUnitId", "OrganizationUnit");
            DropForeignKey("TaskUsage", "ObjectOwnerId", "User");
            DropForeignKey("TaskUsage", "LastChangedByUserId", "User");
            DropForeignKey("TaskRef", "ParentId", "TaskRef");
            DropForeignKey("TaskRef", "OwnedByOrganizationUnitId", "OrganizationUnit");
            DropForeignKey("TaskRef", "ObjectOwnerId", "User");
            DropForeignKey("TaskRef", "LastChangedByUserId", "User");
            DropForeignKey("TaskRefItSystemUsages", "ItSystemUsage_Id", "ItSystemUsage");
            DropForeignKey("TaskRefItSystemUsages", "TaskRef_Id", "TaskRef");
            DropForeignKey("TaskRefItSystems", "ItSystem_Id", "ItSystem");
            DropForeignKey("TaskRefItSystems", "TaskRef_Id", "TaskRef");
            DropForeignKey("ItSystem", "ParentId", "ItSystem");
            DropForeignKey("ItSystem", "OrganizationId", "Organization");
            DropForeignKey("ItSystem", "ObjectOwnerId", "User");
            DropForeignKey("ItSystem", "LastChangedByUserId", "User");
            DropForeignKey("Exhibit", "ItSystemId", "ItSystem");
            DropForeignKey("ItInterfaceUses", "ItSystemId", "ItSystem");
            DropForeignKey("InfUsage", "ItSystemUsageId", "ItSystemUsage");
            DropForeignKey("InfUsage", new[] { "ItSystemId", "ItInterfaceId" }, "ItInterfaceUses");
            DropForeignKey("InfUsage", "ItContractId", "ItContract");
            DropForeignKey("InfUsage", "InfrastructureId", "ItSystemUsage");
            DropForeignKey("DataRowUsage", new[] { "SysUsageId", "SysId", "IntfId" }, "InfUsage");
            DropForeignKey("DataRowUsage", "FrequencyId", "Frequencies");
            DropForeignKey("Frequencies", "ObjectOwnerId", "User");
            DropForeignKey("Frequencies", "LastChangedByUserId", "User");
            DropForeignKey("DataRowUsage", "DataRowId", "DataRow");
            DropForeignKey("DataRow", "ObjectOwnerId", "User");
            DropForeignKey("DataRow", "LastChangedByUserId", "User");
            DropForeignKey("DataRow", "ItInterfaceId", "ItInterface");
            DropForeignKey("ItInterface", "TsaId", "Tsas");
            DropForeignKey("Tsas", "ObjectOwnerId", "User");
            DropForeignKey("Tsas", "LastChangedByUserId", "User");
            DropForeignKey("ItInterface", "OrganizationId", "Organization");
            DropForeignKey("ItInterface", "ObjectOwnerId", "User");
            DropForeignKey("ItInterface", "MethodId", "Methods");
            DropForeignKey("Methods", "ObjectOwnerId", "User");
            DropForeignKey("Methods", "LastChangedByUserId", "User");
            DropForeignKey("ItInterface", "LastChangedByUserId", "User");
            DropForeignKey("ItInterface", "InterfaceTypeId", "InterfaceTypes");
            DropForeignKey("InterfaceTypes", "ObjectOwnerId", "User");
            DropForeignKey("InterfaceTypes", "LastChangedByUserId", "User");
            DropForeignKey("InfUsage", "ItInterfaceId", "ItInterface");
            DropForeignKey("ItInterfaceExhibitUsage", "ItInterface_Id", "ItInterface");
            DropForeignKey("ItInterface", "InterfaceId", "Interfaces");
            DropForeignKey("Interfaces", "ObjectOwnerId", "User");
            DropForeignKey("Interfaces", "LastChangedByUserId", "User");
            DropForeignKey("Exhibit", "ObjectOwnerId", "User");
            DropForeignKey("Exhibit", "LastChangedByUserId", "User");
            DropForeignKey("ItInterfaceExhibitUsage", "ItSystemUsageId", "ItSystemUsage");
            DropForeignKey("ItInterfaceExhibitUsage", "ItInterfaceExhibitId", "Exhibit");
            DropForeignKey("ItInterfaceExhibitUsage", "ItContractId", "ItContract");
            DropForeignKey("ItContract", "TerminationDeadlineId", "TerminationDeadlines");
            DropForeignKey("TerminationDeadlines", "ObjectOwnerId", "User");
            DropForeignKey("TerminationDeadlines", "LastChangedByUserId", "User");
            DropForeignKey("ItContract", "SupplierId", "Organization");
            DropForeignKey("ItContract", "ResponsibleOrganizationUnitId", "OrganizationUnit");
            DropForeignKey("ItContract", "PurchaseFormId", "PurchaseForms");
            DropForeignKey("PurchaseForms", "ObjectOwnerId", "User");
            DropForeignKey("PurchaseForms", "LastChangedByUserId", "User");
            DropForeignKey("ItContract", "ProcurementStrategyId", "ProcurementStrategies");
            DropForeignKey("ProcurementStrategies", "ObjectOwnerId", "User");
            DropForeignKey("ProcurementStrategies", "LastChangedByUserId", "User");
            DropForeignKey("ItContract", "PriceRegulationId", "PriceRegulations");
            DropForeignKey("PriceRegulations", "ObjectOwnerId", "User");
            DropForeignKey("PriceRegulations", "LastChangedByUserId", "User");
            DropForeignKey("ItContract", "PaymentModelId", "PaymentModels");
            DropForeignKey("PaymentModels", "ObjectOwnerId", "User");
            DropForeignKey("PaymentModels", "LastChangedByUserId", "User");
            DropForeignKey("PaymentMilestones", "ObjectOwnerId", "User");
            DropForeignKey("PaymentMilestones", "LastChangedByUserId", "User");
            DropForeignKey("PaymentMilestones", "ItContractId", "ItContract");
            DropForeignKey("ItContract", "PaymentFreqencyId", "PaymentFreqencies");
            DropForeignKey("PaymentFreqencies", "ObjectOwnerId", "User");
            DropForeignKey("PaymentFreqencies", "LastChangedByUserId", "User");
            DropForeignKey("ItContract", "ParentId", "ItContract");
            DropForeignKey("ItContract", "OrganizationId", "Organization");
            DropForeignKey("ItContract", "OptionExtendId", "OptionExtends");
            DropForeignKey("OptionExtends", "ObjectOwnerId", "User");
            DropForeignKey("OptionExtends", "LastChangedByUserId", "User");
            DropForeignKey("ItContract", "ObjectOwnerId", "User");
            DropForeignKey("ItContract", "LastChangedByUserId", "User");
            DropForeignKey("HandoverTrial", "ObjectOwnerId", "User");
            DropForeignKey("HandoverTrial", "LastChangedByUserId", "User");
            DropForeignKey("HandoverTrial", "ItContractId", "ItContract");
            DropForeignKey("HandoverTrial", "HandoverTrialTypeId", "HandoverTrialTypes");
            DropForeignKey("HandoverTrialTypes", "ObjectOwnerId", "User");
            DropForeignKey("HandoverTrialTypes", "LastChangedByUserId", "User");
            DropForeignKey("EconomyStream", "OrganizationUnitId", "OrganizationUnit");
            DropForeignKey("EconomyStream", "ObjectOwnerId", "User");
            DropForeignKey("EconomyStream", "LastChangedByUserId", "User");
            DropForeignKey("EconomyStream", "InternPaymentForId", "ItContract");
            DropForeignKey("EconomyStream", "ExternPaymentForId", "ItContract");
            DropForeignKey("ItContract", "ContractTypeId", "ContractTypes");
            DropForeignKey("ContractTypes", "ObjectOwnerId", "User");
            DropForeignKey("ContractTypes", "LastChangedByUserId", "User");
            DropForeignKey("ItContract", "ContractTemplateId", "ContractTemplates");
            DropForeignKey("ContractTemplates", "ObjectOwnerId", "User");
            DropForeignKey("ContractTemplates", "LastChangedByUserId", "User");
            DropForeignKey("ItContract", "ContractSignerId", "User");
            DropForeignKey("ItContractItSystemUsages", "ItContractId", "ItContract");
            DropForeignKey("ItContractAgreementElements", "ElemId", "AgreementElements");
            DropForeignKey("ItContractAgreementElements", "ItContractId", "ItContract");
            DropForeignKey("AgreementElements", "ObjectOwnerId", "User");
            DropForeignKey("AgreementElements", "LastChangedByUserId", "User");
            DropForeignKey("Advice", "ReceiverId", "ItContractRoles");
            DropForeignKey("Advice", "ObjectOwnerId", "User");
            DropForeignKey("Advice", "LastChangedByUserId", "User");
            DropForeignKey("Advice", "ItContractId", "ItContract");
            DropForeignKey("Advice", "CarbonCopyReceiverId", "ItContractRoles");
            DropForeignKey("ItContractRights", "UserId", "User");
            DropForeignKey("ItContractRights", "RoleId", "ItContractRoles");
            DropForeignKey("ItContractRights", "ObjectOwnerId", "User");
            DropForeignKey("ItContractRights", "ObjectId", "ItContract");
            DropForeignKey("ItContractRights", "LastChangedByUserId", "User");
            DropForeignKey("ItContractRoles", "ObjectOwnerId", "User");
            DropForeignKey("ItContractRoles", "LastChangedByUserId", "User");
            DropForeignKey("Exhibit", "Id", "ItInterface");
            DropForeignKey("ItInterfaceUses", "ItInterfaceId", "ItInterface");
            DropForeignKey("ItInterface", "BelongsToId", "Organization");
            DropForeignKey("DataRow", "DataTypeId", "DataTypes");
            DropForeignKey("DataTypes", "ObjectOwnerId", "User");
            DropForeignKey("DataTypes", "LastChangedByUserId", "User");
            DropForeignKey("ItSystem", "BusinessTypeId", "BusinessTypes");
            DropForeignKey("BusinessTypes", "ObjectOwnerId", "User");
            DropForeignKey("BusinessTypes", "LastChangedByUserId", "User");
            DropForeignKey("ItSystem", "BelongsToId", "Organization");
            DropForeignKey("ItSystem", "AppTypeOptionId", "ItSystemTypeOptions");
            DropForeignKey("ItSystemTypeOptions", "ObjectOwnerId", "User");
            DropForeignKey("ItSystemTypeOptions", "LastChangedByUserId", "User");
            DropForeignKey("ItProject", "ObjectOwnerId", "User");
            DropForeignKey("ItProject", "LastChangedByUserId", "User");
            DropForeignKey("ItProject", "JointMunicipalProjectId", "ItProject");
            DropForeignKey("ItProjectItSystemUsages", "ItSystemUsage_Id", "ItSystemUsage");
            DropForeignKey("ItProjectItSystemUsages", "ItProject_Id", "ItProject");
            DropForeignKey("ItProject", "ItProjectTypeId", "ItProjectTypes");
            DropForeignKey("ItProjectTypes", "ObjectOwnerId", "User");
            DropForeignKey("ItProjectTypes", "LastChangedByUserId", "User");
            DropForeignKey("ItProjectStatus", "ObjectOwnerId", "User");
            DropForeignKey("ItProjectStatus", "LastChangedByUserId", "User");
            DropForeignKey("ItProjectStatus", "AssociatedUserId", "User");
            DropForeignKey("ItProjectStatus", "AssociatedItProjectId", "ItProject");
            DropForeignKey("GoalStatus", "ObjectOwnerId", "User");
            DropForeignKey("GoalStatus", "LastChangedByUserId", "User");
            DropForeignKey("GoalStatus", "Id", "ItProject");
            DropForeignKey("Goal", "ObjectOwnerId", "User");
            DropForeignKey("Goal", "LastChangedByUserId", "User");
            DropForeignKey("Goal", "GoalTypeId", "GoalTypes");
            DropForeignKey("GoalTypes", "ObjectOwnerId", "User");
            DropForeignKey("GoalTypes", "LastChangedByUserId", "User");
            DropForeignKey("Goal", "GoalStatusId", "GoalStatus");
            DropForeignKey("EconomyYears", "ItProjectId", "ItProject");
            DropForeignKey("EconomyYears", "ObjectOwnerId", "User");
            DropForeignKey("EconomyYears", "LastChangedByUserId", "User");
            DropForeignKey("Communication", "ResponsibleUserId", "User");
            DropForeignKey("Communication", "ObjectOwnerId", "User");
            DropForeignKey("Communication", "LastChangedByUserId", "User");
            DropForeignKey("Communication", "ItProjectId", "ItProject");
            DropForeignKey("ItProject", "CommonPublicProjectId", "ItProject");
            DropIndex("OrgUnitSystemUsage", new[] { "OrganizationUnit_Id" });
            DropIndex("OrgUnitSystemUsage", new[] { "ItSystemUsage_Id" });
            DropIndex("HandoverUsers", new[] { "User_Id" });
            DropIndex("HandoverUsers", new[] { "Handover_Id" });
            DropIndex("ItProjectTaskRefs", new[] { "TaskRef_Id" });
            DropIndex("ItProjectTaskRefs", new[] { "ItProject_Id" });
            DropIndex("TaskRefItSystemUsages", new[] { "ItSystemUsage_Id" });
            DropIndex("TaskRefItSystemUsages", new[] { "TaskRef_Id" });
            DropIndex("TaskRefItSystems", new[] { "ItSystem_Id" });
            DropIndex("TaskRefItSystems", new[] { "TaskRef_Id" });
            DropIndex("ItContractAgreementElements", new[] { "ElemId" });
            DropIndex("ItContractAgreementElements", new[] { "ItContractId" });
            DropIndex("ItProjectItSystemUsages", new[] { "ItSystemUsage_Id" });
            DropIndex("ItProjectItSystemUsages", new[] { "ItProject_Id" });
            DropIndex("Text", new[] { "LastChangedByUserId" });
            DropIndex("Text", new[] { "ObjectOwnerId" });
            DropIndex("AdminRoles", new[] { "LastChangedByUserId" });
            DropIndex("AdminRoles", new[] { "ObjectOwnerId" });
            DropIndex("OrganizationRoles", new[] { "LastChangedByUserId" });
            DropIndex("OrganizationRoles", new[] { "ObjectOwnerId" });
            DropIndex("OrganizationRights", new[] { "LastChangedByUserId" });
            DropIndex("OrganizationRights", new[] { "ObjectOwnerId" });
            DropIndex("OrganizationRights", new[] { "ObjectId" });
            DropIndex("OrganizationRights", new[] { "RoleId" });
            DropIndex("OrganizationRights", new[] { "UserId" });
            DropIndex("SensitiveDataTypes", new[] { "LastChangedByUserId" });
            DropIndex("SensitiveDataTypes", new[] { "ObjectOwnerId" });
            DropIndex("ItSystemRoles", new[] { "LastChangedByUserId" });
            DropIndex("ItSystemRoles", new[] { "ObjectOwnerId" });
            DropIndex("ItSystemRights", new[] { "LastChangedByUserId" });
            DropIndex("ItSystemRights", new[] { "ObjectOwnerId" });
            DropIndex("ItSystemRights", new[] { "ObjectId" });
            DropIndex("ItSystemRights", new[] { "RoleId" });
            DropIndex("ItSystemRights", new[] { "UserId" });
            DropIndex("itusageorgusage", new[] { "ResponsibleItSystemUsage_Id" });
            DropIndex("itusageorgusage", new[] { "OrganizationUnitId" });
            DropIndex("itusageorgusage", new[] { "ItSystemUsageId" });
            DropIndex("PasswordResetRequest", new[] { "LastChangedByUserId" });
            DropIndex("PasswordResetRequest", new[] { "ObjectOwnerId" });
            DropIndex("PasswordResetRequest", new[] { "UserId" });
            DropIndex("Stakeholder", new[] { "LastChangedByUserId" });
            DropIndex("Stakeholder", new[] { "ObjectOwnerId" });
            DropIndex("Stakeholder", new[] { "ItProjectId" });
            DropIndex("Risk", new[] { "LastChangedByUserId" });
            DropIndex("Risk", new[] { "ObjectOwnerId" });
            DropIndex("Risk", new[] { "ResponsibleUserId" });
            DropIndex("Risk", new[] { "ItProjectId" });
            DropIndex("ItProjectRoles", new[] { "LastChangedByUserId" });
            DropIndex("ItProjectRoles", new[] { "ObjectOwnerId" });
            DropIndex("ItProjectRights", new[] { "LastChangedByUserId" });
            DropIndex("ItProjectRights", new[] { "ObjectOwnerId" });
            DropIndex("ItProjectRights", new[] { "ObjectId" });
            DropIndex("ItProjectRights", new[] { "RoleId" });
            DropIndex("ItProjectRights", new[] { "UserId" });
            DropIndex("ItProjectOrgUnitUsages", new[] { "ResponsibleItProject_Id" });
            DropIndex("ItProjectOrgUnitUsages", new[] { "OrganizationUnitId" });
            DropIndex("ItProjectOrgUnitUsages", new[] { "ItProjectId" });
            DropIndex("Config", new[] { "LastChangedByUserId" });
            DropIndex("Config", new[] { "ObjectOwnerId" });
            DropIndex("Config", new[] { "Id" });
            DropIndex("Wish", new[] { "ItSystem_Id" });
            DropIndex("Wish", new[] { "LastChangedByUserId" });
            DropIndex("Wish", new[] { "ObjectOwnerId" });
            DropIndex("Wish", new[] { "ItSystemUsageId" });
            DropIndex("Wish", new[] { "UserId" });
            DropIndex("TaskUsage", new[] { "LastChangedByUserId" });
            DropIndex("TaskUsage", new[] { "ObjectOwnerId" });
            DropIndex("TaskUsage", new[] { "ParentId" });
            DropIndex("TaskUsage", new[] { "OrgUnitId" });
            DropIndex("TaskUsage", new[] { "TaskRefId" });
            DropIndex("TaskRef", new[] { "OrganizationUnit_Id" });
            DropIndex("TaskRef", new[] { "LastChangedByUserId" });
            DropIndex("TaskRef", new[] { "ObjectOwnerId" });
            DropIndex("TaskRef", new[] { "OwnedByOrganizationUnitId" });
            DropIndex("TaskRef", new[] { "ParentId" });
            DropIndex("Frequencies", new[] { "LastChangedByUserId" });
            DropIndex("Frequencies", new[] { "ObjectOwnerId" });
            DropIndex("Tsas", new[] { "LastChangedByUserId" });
            DropIndex("Tsas", new[] { "ObjectOwnerId" });
            DropIndex("Methods", new[] { "LastChangedByUserId" });
            DropIndex("Methods", new[] { "ObjectOwnerId" });
            DropIndex("InterfaceTypes", new[] { "LastChangedByUserId" });
            DropIndex("InterfaceTypes", new[] { "ObjectOwnerId" });
            DropIndex("Interfaces", new[] { "LastChangedByUserId" });
            DropIndex("Interfaces", new[] { "ObjectOwnerId" });
            DropIndex("TerminationDeadlines", new[] { "LastChangedByUserId" });
            DropIndex("TerminationDeadlines", new[] { "ObjectOwnerId" });
            DropIndex("PurchaseForms", new[] { "LastChangedByUserId" });
            DropIndex("PurchaseForms", new[] { "ObjectOwnerId" });
            DropIndex("ProcurementStrategies", new[] { "LastChangedByUserId" });
            DropIndex("ProcurementStrategies", new[] { "ObjectOwnerId" });
            DropIndex("PriceRegulations", new[] { "LastChangedByUserId" });
            DropIndex("PriceRegulations", new[] { "ObjectOwnerId" });
            DropIndex("PaymentModels", new[] { "LastChangedByUserId" });
            DropIndex("PaymentModels", new[] { "ObjectOwnerId" });
            DropIndex("PaymentMilestones", new[] { "LastChangedByUserId" });
            DropIndex("PaymentMilestones", new[] { "ObjectOwnerId" });
            DropIndex("PaymentMilestones", new[] { "ItContractId" });
            DropIndex("PaymentFreqencies", new[] { "LastChangedByUserId" });
            DropIndex("PaymentFreqencies", new[] { "ObjectOwnerId" });
            DropIndex("OptionExtends", new[] { "LastChangedByUserId" });
            DropIndex("OptionExtends", new[] { "ObjectOwnerId" });
            DropIndex("HandoverTrialTypes", new[] { "LastChangedByUserId" });
            DropIndex("HandoverTrialTypes", new[] { "ObjectOwnerId" });
            DropIndex("HandoverTrial", new[] { "LastChangedByUserId" });
            DropIndex("HandoverTrial", new[] { "ObjectOwnerId" });
            DropIndex("HandoverTrial", new[] { "HandoverTrialTypeId" });
            DropIndex("HandoverTrial", new[] { "ItContractId" });
            DropIndex("EconomyStream", new[] { "LastChangedByUserId" });
            DropIndex("EconomyStream", new[] { "ObjectOwnerId" });
            DropIndex("EconomyStream", new[] { "OrganizationUnitId" });
            DropIndex("EconomyStream", new[] { "InternPaymentForId" });
            DropIndex("EconomyStream", new[] { "ExternPaymentForId" });
            DropIndex("ContractTypes", new[] { "LastChangedByUserId" });
            DropIndex("ContractTypes", new[] { "ObjectOwnerId" });
            DropIndex("ContractTemplates", new[] { "LastChangedByUserId" });
            DropIndex("ContractTemplates", new[] { "ObjectOwnerId" });
            DropIndex("ItContractItSystemUsages", new[] { "ItSystemUsage_Id" });
            DropIndex("ItContractItSystemUsages", new[] { "ItSystemUsageId" });
            DropIndex("ItContractItSystemUsages", new[] { "ItContractId" });
            DropIndex("AgreementElements", new[] { "LastChangedByUserId" });
            DropIndex("AgreementElements", new[] { "ObjectOwnerId" });
            DropIndex("ItContractRights", new[] { "LastChangedByUserId" });
            DropIndex("ItContractRights", new[] { "ObjectOwnerId" });
            DropIndex("ItContractRights", new[] { "ObjectId" });
            DropIndex("ItContractRights", new[] { "RoleId" });
            DropIndex("ItContractRights", new[] { "UserId" });
            DropIndex("ItContractRoles", new[] { "LastChangedByUserId" });
            DropIndex("ItContractRoles", new[] { "ObjectOwnerId" });
            DropIndex("Advice", new[] { "LastChangedByUserId" });
            DropIndex("Advice", new[] { "ObjectOwnerId" });
            DropIndex("Advice", new[] { "ItContractId" });
            DropIndex("Advice", new[] { "CarbonCopyReceiverId" });
            DropIndex("Advice", new[] { "ReceiverId" });
            DropIndex("ItContract", new[] { "LastChangedByUserId" });
            DropIndex("ItContract", new[] { "ObjectOwnerId" });
            DropIndex("ItContract", new[] { "PriceRegulationId" });
            DropIndex("ItContract", new[] { "PaymentModelId" });
            DropIndex("ItContract", new[] { "PaymentFreqencyId" });
            DropIndex("ItContract", new[] { "OptionExtendId" });
            DropIndex("ItContract", new[] { "TerminationDeadlineId" });
            DropIndex("ItContract", new[] { "ParentId" });
            DropIndex("ItContract", new[] { "PurchaseFormId" });
            DropIndex("ItContract", new[] { "ContractTypeId" });
            DropIndex("ItContract", new[] { "ContractTemplateId" });
            DropIndex("ItContract", new[] { "ProcurementStrategyId" });
            DropIndex("ItContract", new[] { "SupplierId" });
            DropIndex("ItContract", new[] { "OrganizationId" });
            DropIndex("ItContract", new[] { "ResponsibleOrganizationUnitId" });
            DropIndex("ItContract", new[] { "ContractSignerId" });
            DropIndex("ItInterfaceExhibitUsage", new[] { "ItInterface_Id" });
            DropIndex("ItInterfaceExhibitUsage", new[] { "ItContractId" });
            DropIndex("ItInterfaceExhibitUsage", new[] { "ItInterfaceExhibitId" });
            DropIndex("ItInterfaceExhibitUsage", new[] { "ItSystemUsageId" });
            DropIndex("Exhibit", new[] { "LastChangedByUserId" });
            DropIndex("Exhibit", new[] { "ObjectOwnerId" });
            DropIndex("Exhibit", new[] { "ItSystemId" });
            DropIndex("Exhibit", new[] { "Id" });
            DropIndex("ItInterface", new[] { "LastChangedByUserId" });
            DropIndex("ItInterface", new[] { "ObjectOwnerId" });
            DropIndex("ItInterface", new[] { "BelongsToId" });
            DropIndex("ItInterface", new[] { "MethodId" });
            DropIndex("ItInterface", new[] { "TsaId" });
            DropIndex("ItInterface", new[] { "InterfaceTypeId" });
            DropIndex("ItInterface", new[] { "InterfaceId" });
            DropIndex("ItInterface", "IX_NamePerOrg");
            DropIndex("DataTypes", new[] { "LastChangedByUserId" });
            DropIndex("DataTypes", new[] { "ObjectOwnerId" });
            DropIndex("DataRow", new[] { "LastChangedByUserId" });
            DropIndex("DataRow", new[] { "ObjectOwnerId" });
            DropIndex("DataRow", new[] { "DataTypeId" });
            DropIndex("DataRow", new[] { "ItInterfaceId" });
            DropIndex("DataRowUsage", new[] { "FrequencyId" });
            DropIndex("DataRowUsage", new[] { "SysUsageId", "SysId", "IntfId" });
            DropIndex("DataRowUsage", new[] { "DataRowId" });
            DropIndex("InfUsage", new[] { "InfrastructureId" });
            DropIndex("InfUsage", new[] { "ItContractId" });
            DropIndex("InfUsage", new[] { "ItInterfaceId" });
            DropIndex("InfUsage", new[] { "ItSystemId", "ItInterfaceId" });
            DropIndex("InfUsage", new[] { "ItSystemUsageId" });
            DropIndex("ItInterfaceUses", new[] { "ItInterfaceId" });
            DropIndex("ItInterfaceUses", new[] { "ItSystemId" });
            DropIndex("BusinessTypes", new[] { "LastChangedByUserId" });
            DropIndex("BusinessTypes", new[] { "ObjectOwnerId" });
            DropIndex("ItSystemTypeOptions", new[] { "LastChangedByUserId" });
            DropIndex("ItSystemTypeOptions", new[] { "ObjectOwnerId" });
            DropIndex("ItSystem", new[] { "LastChangedByUserId" });
            DropIndex("ItSystem", new[] { "ObjectOwnerId" });
            DropIndex("ItSystem", new[] { "BelongsToId" });
            DropIndex("ItSystem", "IX_NamePerOrg");
            DropIndex("ItSystem", new[] { "BusinessTypeId" });
            DropIndex("ItSystem", new[] { "ParentId" });
            DropIndex("ItSystem", new[] { "AppTypeOptionId" });
            DropIndex("Organization", new[] { "LastChangedByUserId" });
            DropIndex("Organization", new[] { "ObjectOwnerId" });
            DropIndex("Organization", new[] { "Cvr" });
            DropIndex("Organization", new[] { "Name" });
            DropIndex("ItProjectTypes", new[] { "LastChangedByUserId" });
            DropIndex("ItProjectTypes", new[] { "ObjectOwnerId" });
            DropIndex("ItProjectStatus", new[] { "LastChangedByUserId" });
            DropIndex("ItProjectStatus", new[] { "ObjectOwnerId" });
            DropIndex("ItProjectStatus", new[] { "AssociatedItProjectId" });
            DropIndex("ItProjectStatus", new[] { "AssociatedUserId" });
            DropIndex("GoalTypes", new[] { "LastChangedByUserId" });
            DropIndex("GoalTypes", new[] { "ObjectOwnerId" });
            DropIndex("Goal", new[] { "LastChangedByUserId" });
            DropIndex("Goal", new[] { "ObjectOwnerId" });
            DropIndex("Goal", new[] { "GoalStatusId" });
            DropIndex("Goal", new[] { "GoalTypeId" });
            DropIndex("GoalStatus", new[] { "LastChangedByUserId" });
            DropIndex("GoalStatus", new[] { "ObjectOwnerId" });
            DropIndex("GoalStatus", new[] { "Id" });
            DropIndex("EconomyYears", new[] { "LastChangedByUserId" });
            DropIndex("EconomyYears", new[] { "ObjectOwnerId" });
            DropIndex("EconomyYears", new[] { "ItProjectId" });
            DropIndex("Communication", new[] { "LastChangedByUserId" });
            DropIndex("Communication", new[] { "ObjectOwnerId" });
            DropIndex("Communication", new[] { "ItProjectId" });
            DropIndex("Communication", new[] { "ResponsibleUserId" });
            DropIndex("ItProject", new[] { "LastChangedByUserId" });
            DropIndex("ItProject", new[] { "ObjectOwnerId" });
            DropIndex("ItProject", new[] { "OriginalId" });
            DropIndex("ItProject", new[] { "CommonPublicProjectId" });
            DropIndex("ItProject", new[] { "JointMunicipalProjectId" });
            DropIndex("ItProject", new[] { "OrganizationId" });
            DropIndex("ItProject", new[] { "ItProjectTypeId" });
            DropIndex("ItProject", new[] { "ParentId" });
            DropIndex("Handover", new[] { "LastChangedByUserId" });
            DropIndex("Handover", new[] { "ObjectOwnerId" });
            DropIndex("Handover", new[] { "Id" });
            DropIndex("User", new[] { "LastChangedByUserId" });
            DropIndex("User", new[] { "ObjectOwnerId" });
            DropIndex("User", new[] { "Email" });
            DropIndex("ArchiveTypes", new[] { "LastChangedByUserId" });
            DropIndex("ArchiveTypes", new[] { "ObjectOwnerId" });
            DropIndex("ItSystemUsage", new[] { "OrganizationUnit_Id" });
            DropIndex("ItSystemUsage", new[] { "LastChangedByUserId" });
            DropIndex("ItSystemUsage", new[] { "ObjectOwnerId" });
            DropIndex("ItSystemUsage", new[] { "OverviewId" });
            DropIndex("ItSystemUsage", new[] { "SensitiveDataTypeId" });
            DropIndex("ItSystemUsage", new[] { "ArchiveTypeId" });
            DropIndex("ItSystemUsage", new[] { "ItSystemId" });
            DropIndex("ItSystemUsage", new[] { "OrganizationId" });
            DropIndex("OrganizationUnit", new[] { "LastChangedByUserId" });
            DropIndex("OrganizationUnit", new[] { "ObjectOwnerId" });
            DropIndex("OrganizationUnit", new[] { "ParentId" });
            DropIndex("OrganizationUnit", "UniqueLocalId");
            DropIndex("AdminRights", new[] { "LastChangedByUserId" });
            DropIndex("AdminRights", new[] { "ObjectOwnerId" });
            DropIndex("AdminRights", new[] { "DefaultOrgUnitId" });
            DropIndex("AdminRights", new[] { "ObjectId" });
            DropIndex("AdminRights", new[] { "RoleId" });
            DropIndex("AdminRights", new[] { "UserId" });
            DropTable("OrgUnitSystemUsage");
            DropTable("HandoverUsers");
            DropTable("ItProjectTaskRefs");
            DropTable("TaskRefItSystemUsages");
            DropTable("TaskRefItSystems");
            DropTable("ItContractAgreementElements");
            DropTable("ItProjectItSystemUsages");
            DropTable("Text");
            DropTable("AdminRoles");
            DropTable("OrganizationRoles");
            DropTable("OrganizationRights");
            DropTable("SensitiveDataTypes");
            DropTable("ItSystemRoles");
            DropTable("ItSystemRights");
            DropTable("itusageorgusage");
            DropTable("PasswordResetRequest");
            DropTable("Stakeholder");
            DropTable("Risk");
            DropTable("ItProjectRoles");
            DropTable("ItProjectRights");
            DropTable("ItProjectOrgUnitUsages");
            DropTable("Config");
            DropTable("Wish");
            DropTable("TaskUsage");
            DropTable("TaskRef");
            DropTable("Frequencies");
            DropTable("Tsas");
            DropTable("Methods");
            DropTable("InterfaceTypes");
            DropTable("Interfaces");
            DropTable("TerminationDeadlines");
            DropTable("PurchaseForms");
            DropTable("ProcurementStrategies");
            DropTable("PriceRegulations");
            DropTable("PaymentModels");
            DropTable("PaymentMilestones");
            DropTable("PaymentFreqencies");
            DropTable("OptionExtends");
            DropTable("HandoverTrialTypes");
            DropTable("HandoverTrial");
            DropTable("EconomyStream");
            DropTable("ContractTypes");
            DropTable("ContractTemplates");
            DropTable("ItContractItSystemUsages");
            DropTable("AgreementElements");
            DropTable("ItContractRights");
            DropTable("ItContractRoles");
            DropTable("Advice");
            DropTable("ItContract");
            DropTable("ItInterfaceExhibitUsage");
            DropTable("Exhibit");
            DropTable("ItInterface");
            DropTable("DataTypes");
            DropTable("DataRow");
            DropTable("DataRowUsage");
            DropTable("InfUsage");
            DropTable("ItInterfaceUses");
            DropTable("BusinessTypes");
            DropTable("ItSystemTypeOptions");
            DropTable("ItSystem");
            DropTable("Organization");
            DropTable("ItProjectTypes");
            DropTable("ItProjectStatus");
            DropTable("GoalTypes");
            DropTable("Goal");
            DropTable("GoalStatus");
            DropTable("EconomyYears");
            DropTable("Communication");
            DropTable("ItProject");
            DropTable("Handover");
            DropTable("User");
            DropTable("ArchiveTypes");
            DropTable("ItSystemUsage");
            DropTable("OrganizationUnit");
            DropTable("AdminRights");
        }
    }
}
