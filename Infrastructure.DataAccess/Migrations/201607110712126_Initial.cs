namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Advice",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsActive = c.Boolean(nullable: false),
                        Name = c.String(),
                        AlarmDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        SentDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        ReceiverId = c.Int(),
                        CarbonCopyReceiverId = c.Int(),
                        Subject = c.String(),
                        ItContractId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContractRoles", t => t.CarbonCopyReceiverId)
                .ForeignKey("dbo.ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.ItContractRoles", t => t.ReceiverId)
                .Index(t => t.ReceiverId)
                .Index(t => t.CarbonCopyReceiverId)
                .Index(t => t.ItContractId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItContractRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        LastName = c.String(),
                        PhoneNumber = c.String(),
                        Email = c.String(nullable: false, maxLength: 100),
                        Password = c.String(nullable: false),
                        Salt = c.String(nullable: false),
                        IsGlobalAdmin = c.Boolean(nullable: false),
                        Uuid = c.Guid(),
                        LastAdvisDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.Email, unique: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.Id)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItProject",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.String(),
                        Background = c.String(),
                        Note = c.String(),
                        Name = c.String(),
                        Description = c.String(),
                        AccessModifier = c.Int(nullable: false),
                        IsArchived = c.Boolean(nullable: false),
                        Esdh = c.String(),
                        Cmdb = c.String(),
                        Folder = c.String(),
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
                        OriginalId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.CommonPublicProjectId)
                .ForeignKey("dbo.ItProjectTypes", t => t.ItProjectTypeId)
                .ForeignKey("dbo.ItProject", t => t.JointMunicipalProjectId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .ForeignKey("dbo.ItProject", t => t.OriginalId)
                .ForeignKey("dbo.ItProject", t => t.ParentId)
                .Index(t => t.ParentId)
                .Index(t => t.ItProjectTypeId)
                .Index(t => t.OrganizationId)
                .Index(t => t.JointMunicipalProjectId)
                .Index(t => t.CommonPublicProjectId)
                .Index(t => t.OriginalId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.User", t => t.ResponsibleUserId)
                .Index(t => t.ResponsibleUserId)
                .Index(t => t.ItProjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.Id)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GoalStatus", t => t.GoalStatusId, cascadeDelete: true)
                .ForeignKey("dbo.GoalTypes", t => t.GoalTypeId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.GoalTypeId)
                .Index(t => t.GoalStatusId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.GoalTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.AssociatedItProjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.AssociatedUserId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.AssociatedUserId)
                .Index(t => t.AssociatedItProjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItProjectTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItSystemUsage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsStatusActive = c.Boolean(nullable: false),
                        Note = c.String(),
                        LocalSystemId = c.String(),
                        Version = c.String(),
                        EsdhRef = c.String(),
                        CmdbRef = c.String(),
                        DirectoryOrUrlRef = c.String(),
                        LocalCallName = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        ItSystemId = c.Int(nullable: false),
                        ArchiveTypeId = c.Int(),
                        SensitiveDataTypeId = c.Int(),
                        OverviewId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                        OrganizationUnit_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ArchiveTypes", t => t.ArchiveTypeId)
                .ForeignKey("dbo.OrganizationUnit", t => t.OrganizationUnit_Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemUsage", t => t.OverviewId)
                .ForeignKey("dbo.SensitiveDataTypes", t => t.SensitiveDataTypeId)
                .Index(t => t.OrganizationId)
                .Index(t => t.ItSystemId)
                .Index(t => t.ArchiveTypeId)
                .Index(t => t.SensitiveDataTypeId)
                .Index(t => t.OverviewId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.OrganizationUnit_Id);

            CreateTable(
                "dbo.ArchiveTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItContractItSystemUsages",
                c => new
                    {
                        ItContractId = c.Int(nullable: false),
                        ItSystemUsageId = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItContractId, t.ItSystemUsageId })
                .ForeignKey("dbo.ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsageId)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .Index(t => t.ItContractId)
                .Index(t => t.ItSystemUsageId)
                .Index(t => t.ItSystemUsage_Id);

            CreateTable(
                "dbo.ItContract",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Note = c.String(),
                        ItContractId = c.String(),
                        Esdh = c.String(),
                        Folder = c.String(),
                        SupplierContractSigner = c.String(),
                        HasSupplierSigned = c.Boolean(nullable: false),
                        SupplierSignedDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        ContractSignerId = c.Int(),
                        IsSigned = c.Boolean(nullable: false),
                        SignedDate = c.DateTime(precision: 7, storeType: "datetime2"),
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
                        OperationTestExpected = c.DateTime(precision: 7, storeType: "datetime2"),
                        OperationTestApproved = c.DateTime(precision: 7, storeType: "datetime2"),
                        OperationalAcceptanceTestExpected = c.DateTime(precision: 7, storeType: "datetime2"),
                        OperationalAcceptanceTestApproved = c.DateTime(precision: 7, storeType: "datetime2"),
                        Concluded = c.DateTime(precision: 7, storeType: "datetime2"),
                        Duration = c.Int(),
                        IrrevocableTo = c.DateTime(precision: 7, storeType: "datetime2"),
                        ExpirationDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        Terminated = c.DateTime(precision: 7, storeType: "datetime2"),
                        TerminationDeadlineId = c.Int(),
                        OptionExtendId = c.Int(),
                        ExtendMultiplier = c.Int(nullable: false),
                        OperationRemunerationBegun = c.DateTime(precision: 7, storeType: "datetime2"),
                        PaymentFreqencyId = c.Int(),
                        PaymentModelId = c.Int(),
                        PriceRegulationId = c.Int(),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.ContractSignerId)
                .ForeignKey("dbo.ItContractTemplateTypes", t => t.ContractTemplateId)
                .ForeignKey("dbo.ItContractTypes", t => t.ContractTypeId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.OptionExtendTypes", t => t.OptionExtendId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .ForeignKey("dbo.ItContract", t => t.ParentId)
                .ForeignKey("dbo.PaymentFreqencyTypes", t => t.PaymentFreqencyId)
                .ForeignKey("dbo.PaymentModelTypes", t => t.PaymentModelId)
                .ForeignKey("dbo.PriceRegulationTypes", t => t.PriceRegulationId)
                .ForeignKey("dbo.ProcurementStrategyTypes", t => t.ProcurementStrategyId)
                .ForeignKey("dbo.PurchaseFormTypes", t => t.PurchaseFormId)
                .ForeignKey("dbo.OrganizationUnit", t => t.ResponsibleOrganizationUnitId)
                .ForeignKey("dbo.Organization", t => t.SupplierId)
                .ForeignKey("dbo.TerminationDeadlineTypes", t => t.TerminationDeadlineId)
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
                "dbo.AgreementElementTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItInterfaceExhibitUsage",
                c => new
                    {
                        ItSystemUsageId = c.Int(nullable: false),
                        ItInterfaceExhibitId = c.Int(nullable: false),
                        ItContractId = c.Int(),
                        IsWishedFor = c.Boolean(nullable: false),
                        ItInterface_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItSystemUsageId, t.ItInterfaceExhibitId })
                .ForeignKey("dbo.ItContract", t => t.ItContractId)
                .ForeignKey("dbo.ItInterface", t => t.ItInterface_Id)
                .ForeignKey("dbo.Exhibit", t => t.ItInterfaceExhibitId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsageId)
                .Index(t => t.ItSystemUsageId)
                .Index(t => t.ItInterfaceExhibitId)
                .Index(t => t.ItContractId)
                .Index(t => t.ItInterface_Id);

            CreateTable(
                "dbo.Exhibit",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ItSystemId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .ForeignKey("dbo.ItInterface", t => t.Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.Id)
                .Index(t => t.ItSystemId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItInterface",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Version = c.String(maxLength: 20),
                        ItInterfaceId = c.String(nullable: false, maxLength: 100),
                        InterfaceId = c.Int(),
                        InterfaceTypeId = c.Int(),
                        TsaId = c.Int(),
                        MethodId = c.Int(),
                        Note = c.String(),
                        Name = c.String(nullable: false, maxLength: 100),
                        Uuid = c.Guid(nullable: false),
                        Description = c.String(),
                        Url = c.String(),
                        AccessModifier = c.Int(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        BelongsToId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.BelongsToId)
                .ForeignKey("dbo.InterfaceTypes", t => t.InterfaceId)
                .ForeignKey("dbo.ItInterfaceTypes", t => t.InterfaceTypeId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.MethodTypes", t => t.MethodId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .ForeignKey("dbo.TsaTypes", t => t.TsaId)
                .Index(t => new { t.OrganizationId, t.Name, t.ItInterfaceId }, unique: true, name: "UX_NamePerOrg")
                .Index(t => t.InterfaceId)
                .Index(t => t.InterfaceTypeId)
                .Index(t => t.TsaId)
                .Index(t => t.MethodId)
                .Index(t => t.BelongsToId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.Organization",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                        Type = c.Int(),
                        Cvr = c.String(maxLength: 10),
                        AccessModifier = c.Int(nullable: false),
                        Uuid = c.Guid(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.Name)
                .Index(t => t.Cvr)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItSystem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemId = c.String(),
                        AppTypeOptionId = c.Int(),
                        ParentId = c.Int(),
                        BusinessTypeId = c.Int(),
                        Name = c.String(nullable: false, maxLength: 100),
                        Uuid = c.Guid(nullable: false),
                        Description = c.String(),
                        Url = c.String(),
                        AccessModifier = c.Int(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        BelongsToId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemTypes", t => t.AppTypeOptionId)
                .ForeignKey("dbo.Organization", t => t.BelongsToId)
                .ForeignKey("dbo.BusinessTypes", t => t.BusinessTypeId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .ForeignKey("dbo.ItSystem", t => t.ParentId)
                .Index(t => t.AppTypeOptionId)
                .Index(t => t.ParentId)
                .Index(t => t.BusinessTypeId)
                .Index(t => new { t.OrganizationId, t.Name }, unique: true, name: "UX_NamePerOrg")
                .Index(t => t.BelongsToId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItSystemTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.BusinessTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItInterfaceUses",
                c => new
                    {
                        ItSystemId = c.Int(nullable: false),
                        ItInterfaceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItSystemId, t.ItInterfaceId })
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .ForeignKey("dbo.ItInterface", t => t.ItInterfaceId, cascadeDelete: true)
                .Index(t => t.ItSystemId)
                .Index(t => t.ItInterfaceId);

            CreateTable(
                "dbo.ItInterfaceUsage",
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
                .ForeignKey("dbo.ItSystemUsage", t => t.InfrastructureId)
                .ForeignKey("dbo.ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("dbo.ItInterfaceUses", t => new { t.ItSystemId, t.ItInterfaceId })
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsageId)
                .ForeignKey("dbo.ItInterface", t => t.ItInterfaceId, cascadeDelete: true)
                .Index(t => t.ItSystemUsageId)
                .Index(t => new { t.ItSystemId, t.ItInterfaceId })
                .Index(t => t.ItContractId)
                .Index(t => t.InfrastructureId);

            CreateTable(
                "dbo.DataRowUsage",
                c => new
                    {
                        DataRowId = c.Int(nullable: false),
                        ItSystemUsageId = c.Int(nullable: false),
                        ItSystemId = c.Int(nullable: false),
                        ItInterfaceId = c.Int(nullable: false),
                        FrequencyId = c.Int(),
                        Amount = c.Int(),
                        Economy = c.Int(),
                        Price = c.Int(),
                    })
                .PrimaryKey(t => new { t.DataRowId, t.ItSystemUsageId, t.ItSystemId, t.ItInterfaceId })
                .ForeignKey("dbo.DataRow", t => t.DataRowId, cascadeDelete: true)
                .ForeignKey("dbo.FrequencyTypes", t => t.FrequencyId)
                .ForeignKey("dbo.ItInterfaceUsage", t => new { t.ItSystemUsageId, t.ItSystemId, t.ItInterfaceId }, cascadeDelete: true)
                .Index(t => t.DataRowId)
                .Index(t => new { t.ItSystemUsageId, t.ItSystemId, t.ItInterfaceId })
                .Index(t => t.FrequencyId);

            CreateTable(
                "dbo.DataRow",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItInterfaceId = c.Int(nullable: false),
                        DataTypeId = c.Int(),
                        Data = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DataTypes", t => t.DataTypeId)
                .ForeignKey("dbo.ItInterface", t => t.ItInterfaceId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ItInterfaceId)
                .Index(t => t.DataTypeId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.DataTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.FrequencyTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.TaskRef",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccessModifier = c.Int(nullable: false),
                        Uuid = c.Guid(nullable: false),
                        Type = c.String(),
                        TaskKey = c.String(),
                        Description = c.String(),
                        ActiveFrom = c.DateTime(precision: 7, storeType: "datetime2"),
                        ActiveTo = c.DateTime(precision: 7, storeType: "datetime2"),
                        ParentId = c.Int(),
                        OwnedByOrganizationUnitId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                        OrganizationUnit_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.OrganizationUnit", t => t.OrganizationUnit_Id)
                .ForeignKey("dbo.OrganizationUnit", t => t.OwnedByOrganizationUnitId, cascadeDelete: true)
                .ForeignKey("dbo.TaskRef", t => t.ParentId)
                .Index(t => t.ParentId)
                .Index(t => t.OwnedByOrganizationUnitId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.OrganizationUnit_Id);

            CreateTable(
                "dbo.OrganizationUnit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LocalId = c.String(maxLength: 100),
                        Name = c.String(),
                        Ean = c.Long(),
                        ParentId = c.Int(),
                        OrganizationId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("dbo.OrganizationUnit", t => t.ParentId)
                .Index(t => new { t.OrganizationId, t.LocalId }, unique: true, name: "UX_LocalId")
                .Index(t => t.ParentId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.OrganizationRights",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        ObjectId = c.Int(nullable: false),
                        DefaultOrgUnitId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OrganizationUnit", t => t.DefaultOrgUnitId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.Organization", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.OrganizationRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.DefaultOrgUnitId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.OrganizationRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.EconomyStream",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExternPaymentForId = c.Int(),
                        InternPaymentForId = c.Int(),
                        OrganizationUnitId = c.Int(),
                        Acquisition = c.Int(nullable: false),
                        Operation = c.Int(nullable: false),
                        Other = c.Int(nullable: false),
                        AccountingEntry = c.String(),
                        AuditStatus = c.Int(nullable: false),
                        AuditDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContract", t => t.ExternPaymentForId)
                .ForeignKey("dbo.ItContract", t => t.InternPaymentForId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.OrganizationUnit", t => t.OrganizationUnitId)
                .Index(t => t.ExternPaymentForId)
                .Index(t => t.InternPaymentForId)
                .Index(t => t.OrganizationUnitId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.OrganizationUnitRights",
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.OrganizationUnit", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.OrganizationUnitRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.OrganizationUnitRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.TaskUsage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TaskRefId = c.Int(nullable: false),
                        OrgUnitId = c.Int(nullable: false),
                        ParentId = c.Int(),
                        Starred = c.Boolean(nullable: false),
                        TechnologyStatus = c.Int(nullable: false),
                        UsageStatus = c.Int(nullable: false),
                        Comment = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.OrganizationUnit", t => t.OrgUnitId)
                .ForeignKey("dbo.TaskUsage", t => t.ParentId)
                .ForeignKey("dbo.TaskRef", t => t.TaskRefId, cascadeDelete: true)
                .Index(t => t.TaskRefId)
                .Index(t => t.OrgUnitId)
                .Index(t => t.ParentId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItSystemUsageOrgUnitUsages",
                c => new
                    {
                        ItSystemUsageId = c.Int(nullable: false),
                        OrganizationUnitId = c.Int(nullable: false),
                        ResponsibleItSystemUsage_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItSystemUsageId, t.OrganizationUnitId })
                .ForeignKey("dbo.OrganizationUnit", t => t.OrganizationUnitId)
                .ForeignKey("dbo.ItSystemUsage", t => t.ResponsibleItSystemUsage_Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsageId)
                .Index(t => t.ItSystemUsageId)
                .Index(t => t.OrganizationUnitId)
                .Index(t => t.ResponsibleItSystemUsage_Id);

            CreateTable(
                "dbo.ItProjectOrgUnitUsages",
                c => new
                    {
                        ItProjectId = c.Int(nullable: false),
                        OrganizationUnitId = c.Int(nullable: false),
                        ResponsibleItProject_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItProjectId, t.OrganizationUnitId })
                .ForeignKey("dbo.OrganizationUnit", t => t.OrganizationUnitId)
                .ForeignKey("dbo.ItProject", t => t.ResponsibleItProject_Id)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId)
                .Index(t => t.OrganizationUnitId)
                .Index(t => t.ResponsibleItProject_Id);

            CreateTable(
                "dbo.Wish",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsPublic = c.Boolean(nullable: false),
                        Text = c.String(),
                        UserId = c.Int(nullable: false),
                        ItSystemUsageId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                        ItSystem_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsageId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id)
                .Index(t => t.UserId)
                .Index(t => t.ItSystemUsageId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.ItSystem_Id);

            CreateTable(
                "dbo.Config",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ShowItProjectModule = c.Boolean(nullable: false),
                        ShowItSystemModule = c.Boolean(nullable: false),
                        ShowItContractModule = c.Boolean(nullable: false),
                        ItSupportModuleNameId = c.Int(nullable: false),
                        ItSupportGuide = c.String(),
                        ShowTabOverview = c.Boolean(nullable: false),
                        ShowColumnTechnology = c.Boolean(nullable: false),
                        ShowColumnUsage = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.InterfaceTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItInterfaceTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.MethodTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.TsaTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItContractTemplateTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItContractTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.HandoverTrial",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Expected = c.DateTime(precision: 7, storeType: "datetime2"),
                        Approved = c.DateTime(precision: 7, storeType: "datetime2"),
                        ItContractId = c.Int(nullable: false),
                        HandoverTrialTypeId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.HandoverTrialTypes", t => t.HandoverTrialTypeId)
                .ForeignKey("dbo.ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ItContractId)
                .Index(t => t.HandoverTrialTypeId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.HandoverTrialTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.OptionExtendTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.PaymentFreqencyTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.PaymentMilestones",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Expected = c.DateTime(precision: 7, storeType: "datetime2"),
                        Approved = c.DateTime(precision: 7, storeType: "datetime2"),
                        ItContractId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ItContractId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.PaymentModelTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.PriceRegulationTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ProcurementStrategyTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.PurchaseFormTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItContractRights",
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.ItContract", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.ItContractRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.TerminationDeadlineTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItSystemRights",
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.ItSystemUsage", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.ItSystemRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItSystemRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.SensitiveDataTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.ItProject", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.ItProjectRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItProjectRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.User", t => t.ResponsibleUserId)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId)
                .Index(t => t.ResponsibleUserId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.PasswordResetRequest",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Hash = c.String(),
                        Time = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        UserId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.Text",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);

            CreateTable(
                "dbo.ItContractAgreementElementTypes",
                c => new
                    {
                        ItContract_Id = c.Int(nullable: false),
                        AgreementElementType_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItContract_Id, t.AgreementElementType_Id })
                .ForeignKey("dbo.ItContract", t => t.ItContract_Id)
                .ForeignKey("dbo.AgreementElementTypes", t => t.AgreementElementType_Id)
                .Index(t => t.ItContract_Id)
                .Index(t => t.AgreementElementType_Id);

            CreateTable(
                "dbo.TaskRefItSystems",
                c => new
                    {
                        TaskRef_Id = c.Int(nullable: false),
                        ItSystem_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TaskRef_Id, t.ItSystem_Id })
                .ForeignKey("dbo.TaskRef", t => t.TaskRef_Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id)
                .Index(t => t.TaskRef_Id)
                .Index(t => t.ItSystem_Id);

            CreateTable(
                "dbo.TaskRefItSystemUsages",
                c => new
                    {
                        TaskRef_Id = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TaskRef_Id, t.ItSystemUsage_Id })
                .ForeignKey("dbo.TaskRef", t => t.TaskRef_Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .Index(t => t.TaskRef_Id)
                .Index(t => t.ItSystemUsage_Id);

            CreateTable(
                "dbo.ItSystemUsageOrganizationUnits",
                c => new
                    {
                        ItSystemUsage_Id = c.Int(nullable: false),
                        OrganizationUnit_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItSystemUsage_Id, t.OrganizationUnit_Id })
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .ForeignKey("dbo.OrganizationUnit", t => t.OrganizationUnit_Id)
                .Index(t => t.ItSystemUsage_Id)
                .Index(t => t.OrganizationUnit_Id);

            CreateTable(
                "dbo.ItProjectItSystemUsages",
                c => new
                    {
                        ItProject_Id = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItProject_Id, t.ItSystemUsage_Id })
                .ForeignKey("dbo.ItProject", t => t.ItProject_Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .Index(t => t.ItProject_Id)
                .Index(t => t.ItSystemUsage_Id);

            CreateTable(
                "dbo.ItProjectTaskRefs",
                c => new
                    {
                        ItProject_Id = c.Int(nullable: false),
                        TaskRef_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItProject_Id, t.TaskRef_Id })
                .ForeignKey("dbo.ItProject", t => t.ItProject_Id)
                .ForeignKey("dbo.TaskRef", t => t.TaskRef_Id)
                .Index(t => t.ItProject_Id)
                .Index(t => t.TaskRef_Id);

            CreateTable(
                "dbo.HandoverUsers",
                c => new
                    {
                        Handover_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Handover_Id, t.User_Id })
                .ForeignKey("dbo.Handover", t => t.Handover_Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.Handover_Id)
                .Index(t => t.User_Id);

        }

        public override void Down()
        {
            DropForeignKey("dbo.Text", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Text", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Advice", "ReceiverId", "dbo.ItContractRoles");
            DropForeignKey("dbo.Advice", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Advice", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Advice", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.Advice", "CarbonCopyReceiverId", "dbo.ItContractRoles");
            DropForeignKey("dbo.ItContractRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItContractRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.PasswordResetRequest", "UserId", "dbo.User");
            DropForeignKey("dbo.PasswordResetRequest", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.PasswordResetRequest", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.User", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.User", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.HandoverUsers", "User_Id", "dbo.User");
            DropForeignKey("dbo.HandoverUsers", "Handover_Id", "dbo.Handover");
            DropForeignKey("dbo.Handover", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Handover", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Handover", "Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectOrgUnitUsages", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectTaskRefs", "TaskRef_Id", "dbo.TaskRef");
            DropForeignKey("dbo.ItProjectTaskRefs", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.Stakeholder", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.Stakeholder", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Stakeholder", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Risk", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.Risk", "ResponsibleUserId", "dbo.User");
            DropForeignKey("dbo.Risk", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Risk", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectRights", "UserId", "dbo.User");
            DropForeignKey("dbo.ItProjectRights", "RoleId", "dbo.ItProjectRoles");
            DropForeignKey("dbo.ItProjectRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProjectRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectRights", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProjectRights", "ObjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectRights", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectOrgUnitUsages", "ResponsibleItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "ParentId", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "OriginalId", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItProject", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProject", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProject", "JointMunicipalProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectItSystemUsages", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItProjectItSystemUsages", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.ItSystemUsageOrgUnitUsages", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsage", "SensitiveDataTypeId", "dbo.SensitiveDataTypes");
            DropForeignKey("dbo.SensitiveDataTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.SensitiveDataTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemRights", "UserId", "dbo.User");
            DropForeignKey("dbo.ItSystemRights", "RoleId", "dbo.ItSystemRoles");
            DropForeignKey("dbo.ItSystemRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItSystemRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemRights", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItSystemRights", "ObjectId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemRights", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemUsageOrgUnitUsages", "ResponsibleItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsage", "OverviewId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsageOrganizationUnits", "OrganizationUnit_Id", "dbo.OrganizationUnit");
            DropForeignKey("dbo.ItSystemUsageOrganizationUnits", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsage", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItSystemUsage", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItContractItSystemUsages", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsage", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemUsage", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.ItContractItSystemUsages", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItContract", "TerminationDeadlineId", "dbo.TerminationDeadlineTypes");
            DropForeignKey("dbo.TerminationDeadlineTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.TerminationDeadlineTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "SupplierId", "dbo.Organization");
            DropForeignKey("dbo.ItContractRights", "UserId", "dbo.User");
            DropForeignKey("dbo.ItContractRights", "RoleId", "dbo.ItContractRoles");
            DropForeignKey("dbo.ItContractRights", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItContractRights", "ObjectId", "dbo.ItContract");
            DropForeignKey("dbo.ItContractRights", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "ResponsibleOrganizationUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.ItContract", "PurchaseFormId", "dbo.PurchaseFormTypes");
            DropForeignKey("dbo.PurchaseFormTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.PurchaseFormTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "ProcurementStrategyId", "dbo.ProcurementStrategyTypes");
            DropForeignKey("dbo.ProcurementStrategyTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ProcurementStrategyTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "PriceRegulationId", "dbo.PriceRegulationTypes");
            DropForeignKey("dbo.PriceRegulationTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.PriceRegulationTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "PaymentModelId", "dbo.PaymentModelTypes");
            DropForeignKey("dbo.PaymentModelTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.PaymentModelTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.PaymentMilestones", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.PaymentMilestones", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.PaymentMilestones", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.ItContract", "PaymentFreqencyId", "dbo.PaymentFreqencyTypes");
            DropForeignKey("dbo.PaymentFreqencyTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.PaymentFreqencyTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "ParentId", "dbo.ItContract");
            DropForeignKey("dbo.ItContract", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItContract", "OptionExtendId", "dbo.OptionExtendTypes");
            DropForeignKey("dbo.OptionExtendTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.OptionExtendTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItContract", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.HandoverTrial", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.HandoverTrial", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.HandoverTrial", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.HandoverTrial", "HandoverTrialTypeId", "dbo.HandoverTrialTypes");
            DropForeignKey("dbo.HandoverTrialTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.HandoverTrialTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "ContractTypeId", "dbo.ItContractTypes");
            DropForeignKey("dbo.ItContractTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItContractTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "ContractTemplateId", "dbo.ItContractTemplateTypes");
            DropForeignKey("dbo.ItContractTemplateTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItContractTemplateTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "ContractSignerId", "dbo.User");
            DropForeignKey("dbo.ItContractItSystemUsages", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.ItInterfaceExhibitUsage", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItInterfaceExhibitUsage", "ItInterfaceExhibitId", "dbo.Exhibit");
            DropForeignKey("dbo.Exhibit", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Exhibit", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Exhibit", "Id", "dbo.ItInterface");
            DropForeignKey("dbo.ItInterface", "TsaId", "dbo.TsaTypes");
            DropForeignKey("dbo.TsaTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.TsaTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItInterface", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItInterface", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItInterface", "MethodId", "dbo.MethodTypes");
            DropForeignKey("dbo.MethodTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.MethodTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItInterface", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItInterface", "InterfaceTypeId", "dbo.ItInterfaceTypes");
            DropForeignKey("dbo.ItInterfaceTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItInterfaceTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItInterfaceUsage", "ItInterfaceId", "dbo.ItInterface");
            DropForeignKey("dbo.ItInterfaceExhibitUsage", "ItInterface_Id", "dbo.ItInterface");
            DropForeignKey("dbo.ItInterface", "InterfaceId", "dbo.InterfaceTypes");
            DropForeignKey("dbo.InterfaceTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.InterfaceTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItInterfaceUses", "ItInterfaceId", "dbo.ItInterface");
            DropForeignKey("dbo.ItInterface", "BelongsToId", "dbo.Organization");
            DropForeignKey("dbo.Organization", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Organization", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Config", "Id", "dbo.Organization");
            DropForeignKey("dbo.Config", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Config", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Wish", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.Wish", "UserId", "dbo.User");
            DropForeignKey("dbo.Wish", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Wish", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Wish", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.TaskRef", "ParentId", "dbo.TaskRef");
            DropForeignKey("dbo.TaskRef", "OwnedByOrganizationUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.ItProjectOrgUnitUsages", "OrganizationUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.ItSystemUsageOrgUnitUsages", "OrganizationUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.TaskUsage", "TaskRefId", "dbo.TaskRef");
            DropForeignKey("dbo.TaskUsage", "ParentId", "dbo.TaskUsage");
            DropForeignKey("dbo.TaskUsage", "OrgUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.TaskUsage", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.TaskUsage", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.TaskRef", "OrganizationUnit_Id", "dbo.OrganizationUnit");
            DropForeignKey("dbo.OrganizationUnitRights", "UserId", "dbo.User");
            DropForeignKey("dbo.OrganizationUnitRights", "RoleId", "dbo.OrganizationUnitRoles");
            DropForeignKey("dbo.OrganizationUnitRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.OrganizationUnitRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.OrganizationUnitRights", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.OrganizationUnitRights", "ObjectId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.OrganizationUnitRights", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.OrganizationUnit", "ParentId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.OrganizationUnit", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.OrganizationUnit", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.OrganizationUnit", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.EconomyStream", "OrganizationUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.EconomyStream", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.EconomyStream", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.EconomyStream", "InternPaymentForId", "dbo.ItContract");
            DropForeignKey("dbo.EconomyStream", "ExternPaymentForId", "dbo.ItContract");
            DropForeignKey("dbo.ItSystemUsage", "OrganizationUnit_Id", "dbo.OrganizationUnit");
            DropForeignKey("dbo.OrganizationRights", "UserId", "dbo.User");
            DropForeignKey("dbo.OrganizationRights", "RoleId", "dbo.OrganizationRoles");
            DropForeignKey("dbo.OrganizationRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.OrganizationRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.OrganizationRights", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.OrganizationRights", "ObjectId", "dbo.Organization");
            DropForeignKey("dbo.OrganizationRights", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.OrganizationRights", "DefaultOrgUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.TaskRef", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.TaskRef", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.TaskRefItSystemUsages", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.TaskRefItSystemUsages", "TaskRef_Id", "dbo.TaskRef");
            DropForeignKey("dbo.TaskRefItSystems", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.TaskRefItSystems", "TaskRef_Id", "dbo.TaskRef");
            DropForeignKey("dbo.ItSystem", "ParentId", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystem", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItSystem", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItSystem", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Exhibit", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.ItInterfaceUses", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.ItInterfaceUsage", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItInterfaceUsage", new[] { "ItSystemId", "ItInterfaceId" }, "dbo.ItInterfaceUses");
            DropForeignKey("dbo.ItInterfaceUsage", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.ItInterfaceUsage", "InfrastructureId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.DataRowUsage", new[] { "ItSystemUsageId", "ItSystemId", "ItInterfaceId" }, "dbo.ItInterfaceUsage");
            DropForeignKey("dbo.DataRowUsage", "FrequencyId", "dbo.FrequencyTypes");
            DropForeignKey("dbo.FrequencyTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.FrequencyTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataRowUsage", "DataRowId", "dbo.DataRow");
            DropForeignKey("dbo.DataRow", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataRow", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataRow", "ItInterfaceId", "dbo.ItInterface");
            DropForeignKey("dbo.DataRow", "DataTypeId", "dbo.DataTypes");
            DropForeignKey("dbo.DataTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystem", "BusinessTypeId", "dbo.BusinessTypes");
            DropForeignKey("dbo.BusinessTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.BusinessTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystem", "BelongsToId", "dbo.Organization");
            DropForeignKey("dbo.ItSystem", "AppTypeOptionId", "dbo.ItSystemTypes");
            DropForeignKey("dbo.ItSystemTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItSystemTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItInterfaceExhibitUsage", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.ItContractAgreementElementTypes", "AgreementElementType_Id", "dbo.AgreementElementTypes");
            DropForeignKey("dbo.ItContractAgreementElementTypes", "ItContract_Id", "dbo.ItContract");
            DropForeignKey("dbo.AgreementElementTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.AgreementElementTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemUsage", "ArchiveTypeId", "dbo.ArchiveTypes");
            DropForeignKey("dbo.ArchiveTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ArchiveTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProject", "ItProjectTypeId", "dbo.ItProjectTypes");
            DropForeignKey("dbo.ItProjectTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProjectTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectStatus", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProjectStatus", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectStatus", "AssociatedUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectStatus", "AssociatedItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.GoalStatus", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.GoalStatus", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.GoalStatus", "Id", "dbo.ItProject");
            DropForeignKey("dbo.Goal", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Goal", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Goal", "GoalTypeId", "dbo.GoalTypes");
            DropForeignKey("dbo.GoalTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.GoalTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Goal", "GoalStatusId", "dbo.GoalStatus");
            DropForeignKey("dbo.EconomyYears", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.EconomyYears", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.EconomyYears", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Communication", "ResponsibleUserId", "dbo.User");
            DropForeignKey("dbo.Communication", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Communication", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Communication", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "CommonPublicProjectId", "dbo.ItProject");
            DropIndex("dbo.HandoverUsers", new[] { "User_Id" });
            DropIndex("dbo.HandoverUsers", new[] { "Handover_Id" });
            DropIndex("dbo.ItProjectTaskRefs", new[] { "TaskRef_Id" });
            DropIndex("dbo.ItProjectTaskRefs", new[] { "ItProject_Id" });
            DropIndex("dbo.ItProjectItSystemUsages", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.ItProjectItSystemUsages", new[] { "ItProject_Id" });
            DropIndex("dbo.ItSystemUsageOrganizationUnits", new[] { "OrganizationUnit_Id" });
            DropIndex("dbo.ItSystemUsageOrganizationUnits", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.TaskRefItSystemUsages", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.TaskRefItSystemUsages", new[] { "TaskRef_Id" });
            DropIndex("dbo.TaskRefItSystems", new[] { "ItSystem_Id" });
            DropIndex("dbo.TaskRefItSystems", new[] { "TaskRef_Id" });
            DropIndex("dbo.ItContractAgreementElementTypes", new[] { "AgreementElementType_Id" });
            DropIndex("dbo.ItContractAgreementElementTypes", new[] { "ItContract_Id" });
            DropIndex("dbo.Text", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Text", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PasswordResetRequest", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PasswordResetRequest", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PasswordResetRequest", new[] { "UserId" });
            DropIndex("dbo.Stakeholder", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Stakeholder", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Stakeholder", new[] { "ItProjectId" });
            DropIndex("dbo.Risk", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Risk", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Risk", new[] { "ResponsibleUserId" });
            DropIndex("dbo.Risk", new[] { "ItProjectId" });
            DropIndex("dbo.ItProjectRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProjectRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProjectRights", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProjectRights", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProjectRights", new[] { "ObjectId" });
            DropIndex("dbo.ItProjectRights", new[] { "RoleId" });
            DropIndex("dbo.ItProjectRights", new[] { "UserId" });
            DropIndex("dbo.SensitiveDataTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.SensitiveDataTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItSystemRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemRights", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItSystemRights", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemRights", new[] { "ObjectId" });
            DropIndex("dbo.ItSystemRights", new[] { "RoleId" });
            DropIndex("dbo.ItSystemRights", new[] { "UserId" });
            DropIndex("dbo.TerminationDeadlineTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.TerminationDeadlineTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItContractRights", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItContractRights", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItContractRights", new[] { "ObjectId" });
            DropIndex("dbo.ItContractRights", new[] { "RoleId" });
            DropIndex("dbo.ItContractRights", new[] { "UserId" });
            DropIndex("dbo.PurchaseFormTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PurchaseFormTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ProcurementStrategyTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ProcurementStrategyTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PriceRegulationTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PriceRegulationTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PaymentModelTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PaymentModelTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PaymentMilestones", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PaymentMilestones", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PaymentMilestones", new[] { "ItContractId" });
            DropIndex("dbo.PaymentFreqencyTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PaymentFreqencyTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.OptionExtendTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.OptionExtendTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.HandoverTrialTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.HandoverTrialTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.HandoverTrial", new[] { "LastChangedByUserId" });
            DropIndex("dbo.HandoverTrial", new[] { "ObjectOwnerId" });
            DropIndex("dbo.HandoverTrial", new[] { "HandoverTrialTypeId" });
            DropIndex("dbo.HandoverTrial", new[] { "ItContractId" });
            DropIndex("dbo.ItContractTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItContractTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItContractTemplateTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItContractTemplateTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.TsaTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.TsaTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.MethodTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.MethodTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItInterfaceTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItInterfaceTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.InterfaceTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.InterfaceTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Config", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Config", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Config", new[] { "Id" });
            DropIndex("dbo.Wish", new[] { "ItSystem_Id" });
            DropIndex("dbo.Wish", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Wish", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Wish", new[] { "ItSystemUsageId" });
            DropIndex("dbo.Wish", new[] { "UserId" });
            DropIndex("dbo.ItProjectOrgUnitUsages", new[] { "ResponsibleItProject_Id" });
            DropIndex("dbo.ItProjectOrgUnitUsages", new[] { "OrganizationUnitId" });
            DropIndex("dbo.ItProjectOrgUnitUsages", new[] { "ItProjectId" });
            DropIndex("dbo.ItSystemUsageOrgUnitUsages", new[] { "ResponsibleItSystemUsage_Id" });
            DropIndex("dbo.ItSystemUsageOrgUnitUsages", new[] { "OrganizationUnitId" });
            DropIndex("dbo.ItSystemUsageOrgUnitUsages", new[] { "ItSystemUsageId" });
            DropIndex("dbo.TaskUsage", new[] { "LastChangedByUserId" });
            DropIndex("dbo.TaskUsage", new[] { "ObjectOwnerId" });
            DropIndex("dbo.TaskUsage", new[] { "ParentId" });
            DropIndex("dbo.TaskUsage", new[] { "OrgUnitId" });
            DropIndex("dbo.TaskUsage", new[] { "TaskRefId" });
            DropIndex("dbo.OrganizationUnitRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.OrganizationUnitRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.OrganizationUnitRights", new[] { "LastChangedByUserId" });
            DropIndex("dbo.OrganizationUnitRights", new[] { "ObjectOwnerId" });
            DropIndex("dbo.OrganizationUnitRights", new[] { "ObjectId" });
            DropIndex("dbo.OrganizationUnitRights", new[] { "RoleId" });
            DropIndex("dbo.OrganizationUnitRights", new[] { "UserId" });
            DropIndex("dbo.EconomyStream", new[] { "LastChangedByUserId" });
            DropIndex("dbo.EconomyStream", new[] { "ObjectOwnerId" });
            DropIndex("dbo.EconomyStream", new[] { "OrganizationUnitId" });
            DropIndex("dbo.EconomyStream", new[] { "InternPaymentForId" });
            DropIndex("dbo.EconomyStream", new[] { "ExternPaymentForId" });
            DropIndex("dbo.OrganizationRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.OrganizationRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.OrganizationRights", new[] { "LastChangedByUserId" });
            DropIndex("dbo.OrganizationRights", new[] { "ObjectOwnerId" });
            DropIndex("dbo.OrganizationRights", new[] { "DefaultOrgUnitId" });
            DropIndex("dbo.OrganizationRights", new[] { "ObjectId" });
            DropIndex("dbo.OrganizationRights", new[] { "RoleId" });
            DropIndex("dbo.OrganizationRights", new[] { "UserId" });
            DropIndex("dbo.OrganizationUnit", new[] { "LastChangedByUserId" });
            DropIndex("dbo.OrganizationUnit", new[] { "ObjectOwnerId" });
            DropIndex("dbo.OrganizationUnit", new[] { "ParentId" });
            DropIndex("dbo.OrganizationUnit", "UX_LocalId");
            DropIndex("dbo.TaskRef", new[] { "OrganizationUnit_Id" });
            DropIndex("dbo.TaskRef", new[] { "LastChangedByUserId" });
            DropIndex("dbo.TaskRef", new[] { "ObjectOwnerId" });
            DropIndex("dbo.TaskRef", new[] { "OwnedByOrganizationUnitId" });
            DropIndex("dbo.TaskRef", new[] { "ParentId" });
            DropIndex("dbo.FrequencyTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.FrequencyTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataRow", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataRow", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataRow", new[] { "DataTypeId" });
            DropIndex("dbo.DataRow", new[] { "ItInterfaceId" });
            DropIndex("dbo.DataRowUsage", new[] { "FrequencyId" });
            DropIndex("dbo.DataRowUsage", new[] { "ItSystemUsageId", "ItSystemId", "ItInterfaceId" });
            DropIndex("dbo.DataRowUsage", new[] { "DataRowId" });
            DropIndex("dbo.ItInterfaceUsage", new[] { "InfrastructureId" });
            DropIndex("dbo.ItInterfaceUsage", new[] { "ItContractId" });
            DropIndex("dbo.ItInterfaceUsage", new[] { "ItSystemId", "ItInterfaceId" });
            DropIndex("dbo.ItInterfaceUsage", new[] { "ItSystemUsageId" });
            DropIndex("dbo.ItInterfaceUses", new[] { "ItInterfaceId" });
            DropIndex("dbo.ItInterfaceUses", new[] { "ItSystemId" });
            DropIndex("dbo.BusinessTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.BusinessTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItSystemTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystem", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItSystem", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystem", new[] { "BelongsToId" });
            DropIndex("dbo.ItSystem", "UX_NamePerOrg");
            DropIndex("dbo.ItSystem", new[] { "BusinessTypeId" });
            DropIndex("dbo.ItSystem", new[] { "ParentId" });
            DropIndex("dbo.ItSystem", new[] { "AppTypeOptionId" });
            DropIndex("dbo.Organization", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Organization", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Organization", new[] { "Cvr" });
            DropIndex("dbo.Organization", new[] { "Name" });
            DropIndex("dbo.ItInterface", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItInterface", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItInterface", new[] { "BelongsToId" });
            DropIndex("dbo.ItInterface", new[] { "MethodId" });
            DropIndex("dbo.ItInterface", new[] { "TsaId" });
            DropIndex("dbo.ItInterface", new[] { "InterfaceTypeId" });
            DropIndex("dbo.ItInterface", new[] { "InterfaceId" });
            DropIndex("dbo.ItInterface", "UX_NamePerOrg");
            DropIndex("dbo.Exhibit", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Exhibit", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Exhibit", new[] { "ItSystemId" });
            DropIndex("dbo.Exhibit", new[] { "Id" });
            DropIndex("dbo.ItInterfaceExhibitUsage", new[] { "ItInterface_Id" });
            DropIndex("dbo.ItInterfaceExhibitUsage", new[] { "ItContractId" });
            DropIndex("dbo.ItInterfaceExhibitUsage", new[] { "ItInterfaceExhibitId" });
            DropIndex("dbo.ItInterfaceExhibitUsage", new[] { "ItSystemUsageId" });
            DropIndex("dbo.AgreementElementTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.AgreementElementTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItContract", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItContract", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItContract", new[] { "PriceRegulationId" });
            DropIndex("dbo.ItContract", new[] { "PaymentModelId" });
            DropIndex("dbo.ItContract", new[] { "PaymentFreqencyId" });
            DropIndex("dbo.ItContract", new[] { "OptionExtendId" });
            DropIndex("dbo.ItContract", new[] { "TerminationDeadlineId" });
            DropIndex("dbo.ItContract", new[] { "ParentId" });
            DropIndex("dbo.ItContract", new[] { "PurchaseFormId" });
            DropIndex("dbo.ItContract", new[] { "ContractTypeId" });
            DropIndex("dbo.ItContract", new[] { "ContractTemplateId" });
            DropIndex("dbo.ItContract", new[] { "ProcurementStrategyId" });
            DropIndex("dbo.ItContract", new[] { "SupplierId" });
            DropIndex("dbo.ItContract", new[] { "OrganizationId" });
            DropIndex("dbo.ItContract", new[] { "ResponsibleOrganizationUnitId" });
            DropIndex("dbo.ItContract", new[] { "ContractSignerId" });
            DropIndex("dbo.ItContractItSystemUsages", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.ItContractItSystemUsages", new[] { "ItSystemUsageId" });
            DropIndex("dbo.ItContractItSystemUsages", new[] { "ItContractId" });
            DropIndex("dbo.ArchiveTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ArchiveTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemUsage", new[] { "OrganizationUnit_Id" });
            DropIndex("dbo.ItSystemUsage", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItSystemUsage", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemUsage", new[] { "OverviewId" });
            DropIndex("dbo.ItSystemUsage", new[] { "SensitiveDataTypeId" });
            DropIndex("dbo.ItSystemUsage", new[] { "ArchiveTypeId" });
            DropIndex("dbo.ItSystemUsage", new[] { "ItSystemId" });
            DropIndex("dbo.ItSystemUsage", new[] { "OrganizationId" });
            DropIndex("dbo.ItProjectTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProjectTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProjectStatus", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProjectStatus", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProjectStatus", new[] { "AssociatedItProjectId" });
            DropIndex("dbo.ItProjectStatus", new[] { "AssociatedUserId" });
            DropIndex("dbo.GoalTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.GoalTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Goal", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Goal", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Goal", new[] { "GoalStatusId" });
            DropIndex("dbo.Goal", new[] { "GoalTypeId" });
            DropIndex("dbo.GoalStatus", new[] { "LastChangedByUserId" });
            DropIndex("dbo.GoalStatus", new[] { "ObjectOwnerId" });
            DropIndex("dbo.GoalStatus", new[] { "Id" });
            DropIndex("dbo.EconomyYears", new[] { "LastChangedByUserId" });
            DropIndex("dbo.EconomyYears", new[] { "ObjectOwnerId" });
            DropIndex("dbo.EconomyYears", new[] { "ItProjectId" });
            DropIndex("dbo.Communication", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Communication", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Communication", new[] { "ItProjectId" });
            DropIndex("dbo.Communication", new[] { "ResponsibleUserId" });
            DropIndex("dbo.ItProject", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProject", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProject", new[] { "OriginalId" });
            DropIndex("dbo.ItProject", new[] { "CommonPublicProjectId" });
            DropIndex("dbo.ItProject", new[] { "JointMunicipalProjectId" });
            DropIndex("dbo.ItProject", new[] { "OrganizationId" });
            DropIndex("dbo.ItProject", new[] { "ItProjectTypeId" });
            DropIndex("dbo.ItProject", new[] { "ParentId" });
            DropIndex("dbo.Handover", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Handover", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Handover", new[] { "Id" });
            DropIndex("dbo.User", new[] { "LastChangedByUserId" });
            DropIndex("dbo.User", new[] { "ObjectOwnerId" });
            DropIndex("dbo.User", new[] { "Email" });
            DropIndex("dbo.ItContractRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItContractRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Advice", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Advice", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Advice", new[] { "ItContractId" });
            DropIndex("dbo.Advice", new[] { "CarbonCopyReceiverId" });
            DropIndex("dbo.Advice", new[] { "ReceiverId" });
            DropTable("dbo.HandoverUsers");
            DropTable("dbo.ItProjectTaskRefs");
            DropTable("dbo.ItProjectItSystemUsages");
            DropTable("dbo.ItSystemUsageOrganizationUnits");
            DropTable("dbo.TaskRefItSystemUsages");
            DropTable("dbo.TaskRefItSystems");
            DropTable("dbo.ItContractAgreementElementTypes");
            DropTable("dbo.Text");
            DropTable("dbo.PasswordResetRequest");
            DropTable("dbo.Stakeholder");
            DropTable("dbo.Risk");
            DropTable("dbo.ItProjectRoles");
            DropTable("dbo.ItProjectRights");
            DropTable("dbo.SensitiveDataTypes");
            DropTable("dbo.ItSystemRoles");
            DropTable("dbo.ItSystemRights");
            DropTable("dbo.TerminationDeadlineTypes");
            DropTable("dbo.ItContractRights");
            DropTable("dbo.PurchaseFormTypes");
            DropTable("dbo.ProcurementStrategyTypes");
            DropTable("dbo.PriceRegulationTypes");
            DropTable("dbo.PaymentModelTypes");
            DropTable("dbo.PaymentMilestones");
            DropTable("dbo.PaymentFreqencyTypes");
            DropTable("dbo.OptionExtendTypes");
            DropTable("dbo.HandoverTrialTypes");
            DropTable("dbo.HandoverTrial");
            DropTable("dbo.ItContractTypes");
            DropTable("dbo.ItContractTemplateTypes");
            DropTable("dbo.TsaTypes");
            DropTable("dbo.MethodTypes");
            DropTable("dbo.ItInterfaceTypes");
            DropTable("dbo.InterfaceTypes");
            DropTable("dbo.Config");
            DropTable("dbo.Wish");
            DropTable("dbo.ItProjectOrgUnitUsages");
            DropTable("dbo.ItSystemUsageOrgUnitUsages");
            DropTable("dbo.TaskUsage");
            DropTable("dbo.OrganizationUnitRoles");
            DropTable("dbo.OrganizationUnitRights");
            DropTable("dbo.EconomyStream");
            DropTable("dbo.OrganizationRoles");
            DropTable("dbo.OrganizationRights");
            DropTable("dbo.OrganizationUnit");
            DropTable("dbo.TaskRef");
            DropTable("dbo.FrequencyTypes");
            DropTable("dbo.DataTypes");
            DropTable("dbo.DataRow");
            DropTable("dbo.DataRowUsage");
            DropTable("dbo.ItInterfaceUsage");
            DropTable("dbo.ItInterfaceUses");
            DropTable("dbo.BusinessTypes");
            DropTable("dbo.ItSystemTypes");
            DropTable("dbo.ItSystem");
            DropTable("dbo.Organization");
            DropTable("dbo.ItInterface");
            DropTable("dbo.Exhibit");
            DropTable("dbo.ItInterfaceExhibitUsage");
            DropTable("dbo.AgreementElementTypes");
            DropTable("dbo.ItContract");
            DropTable("dbo.ItContractItSystemUsages");
            DropTable("dbo.ArchiveTypes");
            DropTable("dbo.ItSystemUsage");
            DropTable("dbo.ItProjectTypes");
            DropTable("dbo.ItProjectStatus");
            DropTable("dbo.GoalTypes");
            DropTable("dbo.Goal");
            DropTable("dbo.GoalStatus");
            DropTable("dbo.EconomyYears");
            DropTable("dbo.Communication");
            DropTable("dbo.ItProject");
            DropTable("dbo.Handover");
            DropTable("dbo.User");
            DropTable("dbo.ItContractRoles");
            DropTable("dbo.Advice");
        }
    }
}
