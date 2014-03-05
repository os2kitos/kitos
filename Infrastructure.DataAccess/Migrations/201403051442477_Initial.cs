namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Agreement",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContract", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.ItContract",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContractType_Id = c.Int(nullable: false),
                        ContractTemplate_Id = c.Int(nullable: false),
                        PurchaseForm_Id = c.Int(nullable: false),
                        PaymentModel_Id = c.Int(nullable: false),
                        Supplier_Id = c.Int(nullable: false),
                        Municipality_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ContractTemplate", t => t.ContractTemplate_Id, cascadeDelete: true)
                .ForeignKey("dbo.ContractType", t => t.ContractType_Id, cascadeDelete: true)
                .ForeignKey("dbo.Municipality", t => t.Municipality_Id)
                .ForeignKey("dbo.PaymentModel", t => t.PaymentModel_Id, cascadeDelete: true)
                .ForeignKey("dbo.PurchaseForm", t => t.PurchaseForm_Id, cascadeDelete: true)
                .ForeignKey("dbo.Supplier", t => t.Supplier_Id, cascadeDelete: true)
                .Index(t => t.ContractTemplate_Id)
                .Index(t => t.ContractType_Id)
                .Index(t => t.Municipality_Id)
                .Index(t => t.PaymentModel_Id)
                .Index(t => t.PurchaseForm_Id)
                .Index(t => t.Supplier_Id);
            
            CreateTable(
                "dbo.ContractTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ContractType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Municipality",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Configuration",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ItSupportGuide = c.String(unicode: false),
                        ShowTabOverview = c.Boolean(nullable: false),
                        ShowColumnUsage = c.Boolean(nullable: false),
                        ShowColumnMandatory = c.Boolean(nullable: false),
                        ItProjectGuide = c.String(unicode: false),
                        ShowFocusArea = c.Boolean(nullable: false),
                        ShowPortfolio = c.Boolean(nullable: false),
                        ShowBC = c.Boolean(nullable: false),
                        ItSystemGuide = c.String(unicode: false),
                        ShowRightOfUse = c.Boolean(nullable: false),
                        ShowLicense = c.Boolean(nullable: false),
                        ShowOperation = c.Boolean(nullable: false),
                        ShowMaintenance = c.Boolean(nullable: false),
                        ShowSupport = c.Boolean(nullable: false),
                        ShowServerLicense = c.Boolean(nullable: false),
                        ShowServerOperation = c.Boolean(nullable: false),
                        ShowBackup = c.Boolean(nullable: false),
                        ShowSurveillance = c.Boolean(nullable: false),
                        ShowOther = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Municipality", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.ItProject",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Background = c.String(nullable: false, unicode: false),
                        IsTransversal = c.Boolean(nullable: false),
                        IsTermsOfReferenceApproved = c.Boolean(nullable: false),
                        Note = c.String(nullable: false, unicode: false),
                        Name = c.String(nullable: false, unicode: false),
                        ProjectType_Id = c.Int(nullable: false),
                        ProjectCategory_Id = c.Int(nullable: false),
                        Municipality_Id = c.Int(nullable: false),
                        ItProjectOwner_Id = c.Int(),
                        ItProjectLeader_Id = c.Int(),
                        PartItProjectLeader_Id = c.Int(),
                        Consultant_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Municipality", t => t.Municipality_Id)
                .ForeignKey("dbo.ProjectCategory", t => t.ProjectCategory_Id, cascadeDelete: true)
                .ForeignKey("dbo.ProjectType", t => t.ProjectType_Id, cascadeDelete: true)
                .Index(t => t.Municipality_Id)
                .Index(t => t.ProjectCategory_Id)
                .Index(t => t.ProjectType_Id);
            
            CreateTable(
                "dbo.Communication",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProject_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProject_Id, cascadeDelete: true)
                .Index(t => t.ItProject_Id);
            
            CreateTable(
                "dbo.Economy",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProject_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProject_Id, cascadeDelete: true)
                .Index(t => t.ItProject_Id);
            
            CreateTable(
                "dbo.ExtReference",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(nullable: false, unicode: false),
                        ItProject_Id = c.Int(nullable: false),
                        ExtReferenceType_Id = c.Int(nullable: false),
                        ItSystem_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ExtReferenceType", t => t.ExtReferenceType_Id, cascadeDelete: true)
                .ForeignKey("dbo.ItProject", t => t.ItProject_Id, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id, cascadeDelete: true)
                .Index(t => t.ExtReferenceType_Id)
                .Index(t => t.ItProject_Id)
                .Index(t => t.ItSystem_Id);
            
            CreateTable(
                "dbo.ExtReferenceType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItSystem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentItSystem_Id = c.Int(nullable: false),
                        Municipality_Id = c.Int(nullable: false),
                        SystemType_Id = c.Int(nullable: false),
                        InterfaceType_Id = c.Int(nullable: false),
                        ProtocolType_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.InterfaceType", t => t.InterfaceType_Id)
                .ForeignKey("dbo.Municipality", t => t.Municipality_Id)
                .ForeignKey("dbo.ItSystem", t => t.ParentItSystem_Id)
                .ForeignKey("dbo.ProtocolType", t => t.ProtocolType_Id)
                .ForeignKey("dbo.SystemType", t => t.SystemType_Id)
                .Index(t => t.InterfaceType_Id)
                .Index(t => t.Municipality_Id)
                .Index(t => t.ParentItSystem_Id)
                .Index(t => t.ProtocolType_Id)
                .Index(t => t.SystemType_Id);
            
            CreateTable(
                "dbo.BasicData",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystem_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id, cascadeDelete: true)
                .Index(t => t.ItSystem_Id);
            
            CreateTable(
                "dbo.Component",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystem_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id, cascadeDelete: true)
                .Index(t => t.ItSystem_Id);
            
            CreateTable(
                "dbo.Functionality",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Wish",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Functionality_Id = c.Int(),
                        Interface_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Functionality", t => t.Functionality_Id)
                .ForeignKey("dbo.Interface", t => t.Interface_Id)
                .Index(t => t.Functionality_Id)
                .Index(t => t.Interface_Id);
            
            CreateTable(
                "dbo.Interface",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystem_Id = c.Int(nullable: false),
                        Method_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id, cascadeDelete: true)
                .ForeignKey("dbo.Method", t => t.Method_Id, cascadeDelete: true)
                .Index(t => t.ItSystem_Id)
                .Index(t => t.Method_Id);
            
            CreateTable(
                "dbo.Method",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Infrastructure",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Host_Id = c.Int(nullable: false),
                        Supplier_Id = c.Int(nullable: false),
                        Department_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Department", t => t.Department_Id, cascadeDelete: true)
                .ForeignKey("dbo.Host", t => t.Host_Id, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.Id)
                .ForeignKey("dbo.Supplier", t => t.Supplier_Id, cascadeDelete: true)
                .Index(t => t.Department_Id)
                .Index(t => t.Host_Id)
                .Index(t => t.Id)
                .Index(t => t.Supplier_Id);
            
            CreateTable(
                "dbo.Department",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DepartmentRight",
                c => new
                    {
                        Object_Id = c.Int(nullable: false),
                        Role_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Object_Id, t.Role_Id, t.User_Id })
                .ForeignKey("dbo.Department", t => t.Object_Id, cascadeDelete: true)
                .ForeignKey("dbo.DepartmentRole", t => t.Role_Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Object_Id)
                .Index(t => t.Role_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.DepartmentRole",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        Email = c.String(nullable: false, unicode: false),
                        Password = c.String(nullable: false, unicode: false),
                        Salt = c.String(nullable: false, unicode: false),
                        Municipality_Id = c.Int(nullable: false),
                        Role_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Municipality", t => t.Municipality_Id, cascadeDelete: true)
                .ForeignKey("dbo.Role", t => t.Role_Id)
                .Index(t => t.Municipality_Id)
                .Index(t => t.Role_Id);
            
            CreateTable(
                "dbo.ItContractRight",
                c => new
                    {
                        Object_Id = c.Int(nullable: false),
                        Role_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Object_Id, t.Role_Id, t.User_Id })
                .ForeignKey("dbo.ItContract", t => t.Object_Id, cascadeDelete: true)
                .ForeignKey("dbo.ItContractRole", t => t.Role_Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Object_Id)
                .Index(t => t.Role_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.ItContractRole",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PasswordResetRequest",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128, unicode: false, storeType: "nvarchar"),
                        Time = c.DateTime(nullable: false, precision: 0),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.ItProjectRight",
                c => new
                    {
                        Object_Id = c.Int(nullable: false),
                        Role_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Object_Id, t.Role_Id, t.User_Id })
                .ForeignKey("dbo.ItProject", t => t.Object_Id, cascadeDelete: true)
                .ForeignKey("dbo.ItProjectRole", t => t.Role_Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Object_Id)
                .Index(t => t.Role_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.ItProjectRole",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Role",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItSystemRight",
                c => new
                    {
                        Object_Id = c.Int(nullable: false),
                        Role_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Object_Id, t.Role_Id, t.User_Id })
                .ForeignKey("dbo.ItSystem", t => t.Object_Id, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemRole", t => t.Role_Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Object_Id)
                .Index(t => t.Role_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.ItSystemRole",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Host",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Supplier",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InterfaceType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.KLE",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProject_Id = c.Int(nullable: false),
                        ItSystem_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProject_Id, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id, cascadeDelete: true)
                .Index(t => t.ItProject_Id)
                .Index(t => t.ItSystem_Id);
            
            CreateTable(
                "dbo.ProtocolType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SuperUser",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystem_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id, cascadeDelete: true)
                .Index(t => t.ItSystem_Id);
            
            CreateTable(
                "dbo.SystemType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TaskSupport",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystem_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id, cascadeDelete: true)
                .Index(t => t.ItSystem_Id);
            
            CreateTable(
                "dbo.Technology",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        DatabaseType_Id = c.Int(nullable: false),
                        Environment_Id = c.Int(nullable: false),
                        ProgLanguage_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DatabaseType", t => t.DatabaseType_Id, cascadeDelete: true)
                .ForeignKey("dbo.Environment", t => t.Environment_Id, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.Id)
                .ForeignKey("dbo.ProgLanguage", t => t.ProgLanguage_Id, cascadeDelete: true)
                .Index(t => t.DatabaseType_Id)
                .Index(t => t.Environment_Id)
                .Index(t => t.Id)
                .Index(t => t.ProgLanguage_Id);
            
            CreateTable(
                "dbo.DatabaseType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Environment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProgLanguage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserAdministration",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.GoalStatus",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Goal",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GoalStatus_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GoalStatus", t => t.GoalStatus_Id, cascadeDelete: true)
                .Index(t => t.GoalStatus_Id);
            
            CreateTable(
                "dbo.Handover",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Hierarchy",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ItProgramRef_Id = c.Int(),
                        ItProjectRef_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProgramRef_Id)
                .ForeignKey("dbo.ItProject", t => t.Id, cascadeDelete: true)
                .ForeignKey("dbo.ItProject", t => t.ItProjectRef_Id)
                .Index(t => t.ItProgramRef_Id)
                .Index(t => t.Id)
                .Index(t => t.ItProjectRef_Id);
            
            CreateTable(
                "dbo.Organization",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.PreAnalysis",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.ProjectCategory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProjectStatus",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ProjectPhase_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.Id)
                .ForeignKey("dbo.ProjectPhase", t => t.ProjectPhase_Id)
                .Index(t => t.Id)
                .Index(t => t.ProjectPhase_Id);
            
            CreateTable(
                "dbo.Milestone",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProjectStatus_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProjectStatus", t => t.ProjectStatus_Id, cascadeDelete: true)
                .Index(t => t.ProjectStatus_Id);
            
            CreateTable(
                "dbo.ProjectPhase",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProjectType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Resource",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProject_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProject_Id, cascadeDelete: true)
                .Index(t => t.ItProject_Id);
            
            CreateTable(
                "dbo.Risk",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProject_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProject_Id, cascadeDelete: true)
                .Index(t => t.ItProject_Id);
            
            CreateTable(
                "dbo.Stakeholder",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProject_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProject_Id, cascadeDelete: true)
                .Index(t => t.ItProject_Id);
            
            CreateTable(
                "dbo.ProjectPhaseLocale",
                c => new
                    {
                        Municipality_Id = c.Int(nullable: false),
                        ProjectPhase_Id = c.Int(nullable: false),
                        Name = c.String(unicode: false),
                    })
                .PrimaryKey(t => new { t.Municipality_Id, t.ProjectPhase_Id })
                .ForeignKey("dbo.Municipality", t => t.Municipality_Id, cascadeDelete: true)
                .Index(t => t.Municipality_Id);
            
            CreateTable(
                "dbo.Payment",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContract", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.PaymentModel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PurchaseForm",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ShipNotice",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AlarmDate = c.String(nullable: false, unicode: false),
                        To = c.String(nullable: false, unicode: false),
                        Cc = c.String(nullable: false, unicode: false),
                        Subject = c.String(nullable: false, unicode: false),
                        ItContract_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContract", t => t.ItContract_Id, cascadeDelete: true)
                .Index(t => t.ItContract_Id);
            
            CreateTable(
                "dbo.Localization",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EsdhRef = c.String(unicode: false),
                        CmdbRef = c.String(unicode: false),
                        IdmRef = c.String(unicode: false),
                        FolderRef = c.String(unicode: false),
                        ItProject = c.String(unicode: false),
                        ItProgram = c.String(unicode: false),
                        FocusArea = c.String(unicode: false),
                        Fase1 = c.String(unicode: false),
                        Fase2 = c.String(unicode: false),
                        Fase3 = c.String(unicode: false),
                        Fase4 = c.String(unicode: false),
                        Fase5 = c.String(unicode: false),
                        RightOfUse = c.String(unicode: false),
                        License = c.String(unicode: false),
                        Operation = c.String(unicode: false),
                        Maintenance = c.String(unicode: false),
                        Support = c.String(unicode: false),
                        ServerLicense = c.String(unicode: false),
                        ServerOperation = c.String(unicode: false),
                        Backup = c.String(unicode: false),
                        Surveillance = c.String(unicode: false),
                        Other = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Text",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128, unicode: false, storeType: "nvarchar"),
                        Value = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Agreement", "Id", "dbo.ItContract");
            DropForeignKey("dbo.ItContract", "Supplier_Id", "dbo.Supplier");
            DropForeignKey("dbo.ShipNotice", "ItContract_Id", "dbo.ItContract");
            DropForeignKey("dbo.ItContract", "PurchaseForm_Id", "dbo.PurchaseForm");
            DropForeignKey("dbo.ItContract", "PaymentModel_Id", "dbo.PaymentModel");
            DropForeignKey("dbo.Payment", "Id", "dbo.ItContract");
            DropForeignKey("dbo.ItContract", "Municipality_Id", "dbo.Municipality");
            DropForeignKey("dbo.ProjectPhaseLocale", "Municipality_Id", "dbo.Municipality");
            DropForeignKey("dbo.Stakeholder", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.Risk", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.Resource", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "ProjectType_Id", "dbo.ProjectType");
            DropForeignKey("dbo.ProjectStatus", "ProjectPhase_Id", "dbo.ProjectPhase");
            DropForeignKey("dbo.Milestone", "ProjectStatus_Id", "dbo.ProjectStatus");
            DropForeignKey("dbo.ProjectStatus", "Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "ProjectCategory_Id", "dbo.ProjectCategory");
            DropForeignKey("dbo.PreAnalysis", "Id", "dbo.ItProject");
            DropForeignKey("dbo.Organization", "Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "Municipality_Id", "dbo.Municipality");
            DropForeignKey("dbo.Hierarchy", "ItProjectRef_Id", "dbo.ItProject");
            DropForeignKey("dbo.Hierarchy", "Id", "dbo.ItProject");
            DropForeignKey("dbo.Hierarchy", "ItProgramRef_Id", "dbo.ItProject");
            DropForeignKey("dbo.Handover", "Id", "dbo.ItProject");
            DropForeignKey("dbo.GoalStatus", "Id", "dbo.ItProject");
            DropForeignKey("dbo.Goal", "GoalStatus_Id", "dbo.GoalStatus");
            DropForeignKey("dbo.ExtReference", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.UserAdministration", "Id", "dbo.ItSystem");
            DropForeignKey("dbo.Technology", "ProgLanguage_Id", "dbo.ProgLanguage");
            DropForeignKey("dbo.Technology", "Id", "dbo.ItSystem");
            DropForeignKey("dbo.Technology", "Environment_Id", "dbo.Environment");
            DropForeignKey("dbo.Technology", "DatabaseType_Id", "dbo.DatabaseType");
            DropForeignKey("dbo.TaskSupport", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystem", "SystemType_Id", "dbo.SystemType");
            DropForeignKey("dbo.SuperUser", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystem", "ProtocolType_Id", "dbo.ProtocolType");
            DropForeignKey("dbo.ItSystem", "ParentItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystem", "Municipality_Id", "dbo.Municipality");
            DropForeignKey("dbo.KLE", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.KLE", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.ItSystem", "InterfaceType_Id", "dbo.InterfaceType");
            DropForeignKey("dbo.Infrastructure", "Supplier_Id", "dbo.Supplier");
            DropForeignKey("dbo.Infrastructure", "Id", "dbo.ItSystem");
            DropForeignKey("dbo.Infrastructure", "Host_Id", "dbo.Host");
            DropForeignKey("dbo.Infrastructure", "Department_Id", "dbo.Department");
            DropForeignKey("dbo.DepartmentRight", "User_Id", "dbo.User");
            DropForeignKey("dbo.ItSystemRight", "User_Id", "dbo.User");
            DropForeignKey("dbo.ItSystemRight", "Role_Id", "dbo.ItSystemRole");
            DropForeignKey("dbo.ItSystemRight", "Object_Id", "dbo.ItSystem");
            DropForeignKey("dbo.User", "Role_Id", "dbo.Role");
            DropForeignKey("dbo.ItProjectRight", "User_Id", "dbo.User");
            DropForeignKey("dbo.ItProjectRight", "Role_Id", "dbo.ItProjectRole");
            DropForeignKey("dbo.ItProjectRight", "Object_Id", "dbo.ItProject");
            DropForeignKey("dbo.PasswordResetRequest", "User_Id", "dbo.User");
            DropForeignKey("dbo.User", "Municipality_Id", "dbo.Municipality");
            DropForeignKey("dbo.ItContractRight", "User_Id", "dbo.User");
            DropForeignKey("dbo.ItContractRight", "Role_Id", "dbo.ItContractRole");
            DropForeignKey("dbo.ItContractRight", "Object_Id", "dbo.ItContract");
            DropForeignKey("dbo.DepartmentRight", "Role_Id", "dbo.DepartmentRole");
            DropForeignKey("dbo.DepartmentRight", "Object_Id", "dbo.Department");
            DropForeignKey("dbo.Wish", "Interface_Id", "dbo.Interface");
            DropForeignKey("dbo.Interface", "Method_Id", "dbo.Method");
            DropForeignKey("dbo.Interface", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.Wish", "Functionality_Id", "dbo.Functionality");
            DropForeignKey("dbo.Functionality", "Id", "dbo.ItSystem");
            DropForeignKey("dbo.Component", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.BasicData", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.ExtReference", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.ExtReference", "ExtReferenceType_Id", "dbo.ExtReferenceType");
            DropForeignKey("dbo.Economy", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.Communication", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.Configuration", "Id", "dbo.Municipality");
            DropForeignKey("dbo.ItContract", "ContractType_Id", "dbo.ContractType");
            DropForeignKey("dbo.ItContract", "ContractTemplate_Id", "dbo.ContractTemplate");
            DropIndex("dbo.Agreement", new[] { "Id" });
            DropIndex("dbo.ItContract", new[] { "Supplier_Id" });
            DropIndex("dbo.ShipNotice", new[] { "ItContract_Id" });
            DropIndex("dbo.ItContract", new[] { "PurchaseForm_Id" });
            DropIndex("dbo.ItContract", new[] { "PaymentModel_Id" });
            DropIndex("dbo.Payment", new[] { "Id" });
            DropIndex("dbo.ItContract", new[] { "Municipality_Id" });
            DropIndex("dbo.ProjectPhaseLocale", new[] { "Municipality_Id" });
            DropIndex("dbo.Stakeholder", new[] { "ItProject_Id" });
            DropIndex("dbo.Risk", new[] { "ItProject_Id" });
            DropIndex("dbo.Resource", new[] { "ItProject_Id" });
            DropIndex("dbo.ItProject", new[] { "ProjectType_Id" });
            DropIndex("dbo.ProjectStatus", new[] { "ProjectPhase_Id" });
            DropIndex("dbo.Milestone", new[] { "ProjectStatus_Id" });
            DropIndex("dbo.ProjectStatus", new[] { "Id" });
            DropIndex("dbo.ItProject", new[] { "ProjectCategory_Id" });
            DropIndex("dbo.PreAnalysis", new[] { "Id" });
            DropIndex("dbo.Organization", new[] { "Id" });
            DropIndex("dbo.ItProject", new[] { "Municipality_Id" });
            DropIndex("dbo.Hierarchy", new[] { "ItProjectRef_Id" });
            DropIndex("dbo.Hierarchy", new[] { "Id" });
            DropIndex("dbo.Hierarchy", new[] { "ItProgramRef_Id" });
            DropIndex("dbo.Handover", new[] { "Id" });
            DropIndex("dbo.GoalStatus", new[] { "Id" });
            DropIndex("dbo.Goal", new[] { "GoalStatus_Id" });
            DropIndex("dbo.ExtReference", new[] { "ItSystem_Id" });
            DropIndex("dbo.UserAdministration", new[] { "Id" });
            DropIndex("dbo.Technology", new[] { "ProgLanguage_Id" });
            DropIndex("dbo.Technology", new[] { "Id" });
            DropIndex("dbo.Technology", new[] { "Environment_Id" });
            DropIndex("dbo.Technology", new[] { "DatabaseType_Id" });
            DropIndex("dbo.TaskSupport", new[] { "ItSystem_Id" });
            DropIndex("dbo.ItSystem", new[] { "SystemType_Id" });
            DropIndex("dbo.SuperUser", new[] { "ItSystem_Id" });
            DropIndex("dbo.ItSystem", new[] { "ProtocolType_Id" });
            DropIndex("dbo.ItSystem", new[] { "ParentItSystem_Id" });
            DropIndex("dbo.ItSystem", new[] { "Municipality_Id" });
            DropIndex("dbo.KLE", new[] { "ItSystem_Id" });
            DropIndex("dbo.KLE", new[] { "ItProject_Id" });
            DropIndex("dbo.ItSystem", new[] { "InterfaceType_Id" });
            DropIndex("dbo.Infrastructure", new[] { "Supplier_Id" });
            DropIndex("dbo.Infrastructure", new[] { "Id" });
            DropIndex("dbo.Infrastructure", new[] { "Host_Id" });
            DropIndex("dbo.Infrastructure", new[] { "Department_Id" });
            DropIndex("dbo.DepartmentRight", new[] { "User_Id" });
            DropIndex("dbo.ItSystemRight", new[] { "User_Id" });
            DropIndex("dbo.ItSystemRight", new[] { "Role_Id" });
            DropIndex("dbo.ItSystemRight", new[] { "Object_Id" });
            DropIndex("dbo.User", new[] { "Role_Id" });
            DropIndex("dbo.ItProjectRight", new[] { "User_Id" });
            DropIndex("dbo.ItProjectRight", new[] { "Role_Id" });
            DropIndex("dbo.ItProjectRight", new[] { "Object_Id" });
            DropIndex("dbo.PasswordResetRequest", new[] { "User_Id" });
            DropIndex("dbo.User", new[] { "Municipality_Id" });
            DropIndex("dbo.ItContractRight", new[] { "User_Id" });
            DropIndex("dbo.ItContractRight", new[] { "Role_Id" });
            DropIndex("dbo.ItContractRight", new[] { "Object_Id" });
            DropIndex("dbo.DepartmentRight", new[] { "Role_Id" });
            DropIndex("dbo.DepartmentRight", new[] { "Object_Id" });
            DropIndex("dbo.Wish", new[] { "Interface_Id" });
            DropIndex("dbo.Interface", new[] { "Method_Id" });
            DropIndex("dbo.Interface", new[] { "ItSystem_Id" });
            DropIndex("dbo.Wish", new[] { "Functionality_Id" });
            DropIndex("dbo.Functionality", new[] { "Id" });
            DropIndex("dbo.Component", new[] { "ItSystem_Id" });
            DropIndex("dbo.BasicData", new[] { "ItSystem_Id" });
            DropIndex("dbo.ExtReference", new[] { "ItProject_Id" });
            DropIndex("dbo.ExtReference", new[] { "ExtReferenceType_Id" });
            DropIndex("dbo.Economy", new[] { "ItProject_Id" });
            DropIndex("dbo.Communication", new[] { "ItProject_Id" });
            DropIndex("dbo.Configuration", new[] { "Id" });
            DropIndex("dbo.ItContract", new[] { "ContractType_Id" });
            DropIndex("dbo.ItContract", new[] { "ContractTemplate_Id" });
            DropTable("dbo.Text");
            DropTable("dbo.Localization");
            DropTable("dbo.ShipNotice");
            DropTable("dbo.PurchaseForm");
            DropTable("dbo.PaymentModel");
            DropTable("dbo.Payment");
            DropTable("dbo.ProjectPhaseLocale");
            DropTable("dbo.Stakeholder");
            DropTable("dbo.Risk");
            DropTable("dbo.Resource");
            DropTable("dbo.ProjectType");
            DropTable("dbo.ProjectPhase");
            DropTable("dbo.Milestone");
            DropTable("dbo.ProjectStatus");
            DropTable("dbo.ProjectCategory");
            DropTable("dbo.PreAnalysis");
            DropTable("dbo.Organization");
            DropTable("dbo.Hierarchy");
            DropTable("dbo.Handover");
            DropTable("dbo.Goal");
            DropTable("dbo.GoalStatus");
            DropTable("dbo.UserAdministration");
            DropTable("dbo.ProgLanguage");
            DropTable("dbo.Environment");
            DropTable("dbo.DatabaseType");
            DropTable("dbo.Technology");
            DropTable("dbo.TaskSupport");
            DropTable("dbo.SystemType");
            DropTable("dbo.SuperUser");
            DropTable("dbo.ProtocolType");
            DropTable("dbo.KLE");
            DropTable("dbo.InterfaceType");
            DropTable("dbo.Supplier");
            DropTable("dbo.Host");
            DropTable("dbo.ItSystemRole");
            DropTable("dbo.ItSystemRight");
            DropTable("dbo.Role");
            DropTable("dbo.ItProjectRole");
            DropTable("dbo.ItProjectRight");
            DropTable("dbo.PasswordResetRequest");
            DropTable("dbo.ItContractRole");
            DropTable("dbo.ItContractRight");
            DropTable("dbo.User");
            DropTable("dbo.DepartmentRole");
            DropTable("dbo.DepartmentRight");
            DropTable("dbo.Department");
            DropTable("dbo.Infrastructure");
            DropTable("dbo.Method");
            DropTable("dbo.Interface");
            DropTable("dbo.Wish");
            DropTable("dbo.Functionality");
            DropTable("dbo.Component");
            DropTable("dbo.BasicData");
            DropTable("dbo.ItSystem");
            DropTable("dbo.ExtReferenceType");
            DropTable("dbo.ExtReference");
            DropTable("dbo.Economy");
            DropTable("dbo.Communication");
            DropTable("dbo.ItProject");
            DropTable("dbo.Configuration");
            DropTable("dbo.Municipality");
            DropTable("dbo.ContractType");
            DropTable("dbo.ContractTemplate");
            DropTable("dbo.ItContract");
            DropTable("dbo.Agreement");
        }
    }
}
