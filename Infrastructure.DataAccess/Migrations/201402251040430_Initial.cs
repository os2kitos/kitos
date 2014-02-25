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
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ContractType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Municipality",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Configuration",
                c => new
                    {
                        Id = c.Int(nullable: false),
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
                .ForeignKey("dbo.Person", t => t.Consultant_Id)
                .ForeignKey("dbo.Person", t => t.ItProjectLeader_Id)
                .ForeignKey("dbo.Person", t => t.ItProjectOwner_Id)
                .ForeignKey("dbo.Municipality", t => t.Municipality_Id)
                .ForeignKey("dbo.Person", t => t.PartItProjectLeader_Id)
                .ForeignKey("dbo.ProjectCategory", t => t.ProjectCategory_Id, cascadeDelete: true)
                .ForeignKey("dbo.ProjectType", t => t.ProjectType_Id, cascadeDelete: true)
                .Index(t => t.Consultant_Id)
                .Index(t => t.ItProjectLeader_Id)
                .Index(t => t.ItProjectOwner_Id)
                .Index(t => t.Municipality_Id)
                .Index(t => t.PartItProjectLeader_Id)
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
                "dbo.Person",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        Municipality_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Municipality", t => t.Municipality_Id, cascadeDelete: true)
                .Index(t => t.Municipality_Id);
            
            CreateTable(
                "dbo.ItSystem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentItSystem_Id = c.Int(nullable: false),
                        Municipality_Id = c.Int(nullable: false),
                        Person_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Municipality", t => t.Municipality_Id)
                .ForeignKey("dbo.ItSystem", t => t.ParentItSystem_Id)
                .ForeignKey("dbo.Person", t => t.Person_Id)
                .Index(t => t.Municipality_Id)
                .Index(t => t.ParentItSystem_Id)
                .Index(t => t.Person_Id);
            
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
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id, cascadeDelete: true)
                .Index(t => t.ItSystem_Id);
            
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
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Environment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
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
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProjectStatus",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.Id)
                .Index(t => t.Id);
            
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
                "dbo.ProjectType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
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
                "dbo.Localization",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Literal = c.String(nullable: false, unicode: false),
                        Value = c.String(nullable: false, unicode: false),
                        Municipality_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Municipality", t => t.Municipality_Id, cascadeDelete: true)
                .Index(t => t.Municipality_Id);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        Email = c.String(nullable: false, unicode: false),
                        Password = c.String(nullable: false, unicode: false),
                        Municipality_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Municipality", t => t.Municipality_Id)
                .Index(t => t.Municipality_Id);
            
            CreateTable(
                "dbo.PasswordResetRequest",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Hash = c.String(unicode: false),
                        Time = c.DateTime(nullable: false, precision: 0),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Role",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
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
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PurchaseForm",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
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
                "dbo.Text",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128, unicode: false, storeType: "nvarchar"),
                        Description = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        User_Id = c.Int(nullable: false),
                        Role_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.Role_Id })
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.Role", t => t.Role_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.Role_Id);
            
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
            DropForeignKey("dbo.UserRoles", "Role_Id", "dbo.Role");
            DropForeignKey("dbo.UserRoles", "User_Id", "dbo.User");
            DropForeignKey("dbo.PasswordResetRequest", "User_Id", "dbo.User");
            DropForeignKey("dbo.User", "Municipality_Id", "dbo.Municipality");
            DropForeignKey("dbo.Localization", "Municipality_Id", "dbo.Municipality");
            DropForeignKey("dbo.Stakeholder", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.Risk", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.Resource", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "ProjectType_Id", "dbo.ProjectType");
            DropForeignKey("dbo.Milestone", "ProjectStatus_Id", "dbo.ProjectStatus");
            DropForeignKey("dbo.ProjectStatus", "Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "ProjectCategory_Id", "dbo.ProjectCategory");
            DropForeignKey("dbo.PreAnalysis", "Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "PartItProjectLeader_Id", "dbo.Person");
            DropForeignKey("dbo.Organization", "Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "Municipality_Id", "dbo.Municipality");
            DropForeignKey("dbo.ItProject", "ItProjectOwner_Id", "dbo.Person");
            DropForeignKey("dbo.ItProject", "ItProjectLeader_Id", "dbo.Person");
            DropForeignKey("dbo.Hierarchy", "ItProjectRef_Id", "dbo.ItProject");
            DropForeignKey("dbo.Hierarchy", "Id", "dbo.ItProject");
            DropForeignKey("dbo.Hierarchy", "ItProgramRef_Id", "dbo.ItProject");
            DropForeignKey("dbo.Handover", "Id", "dbo.ItProject");
            DropForeignKey("dbo.GoalStatus", "Id", "dbo.ItProject");
            DropForeignKey("dbo.Goal", "GoalStatus_Id", "dbo.GoalStatus");
            DropForeignKey("dbo.Economy", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "Consultant_Id", "dbo.Person");
            DropForeignKey("dbo.Person", "Municipality_Id", "dbo.Municipality");
            DropForeignKey("dbo.ItSystem", "Person_Id", "dbo.Person");
            DropForeignKey("dbo.UserAdministration", "Id", "dbo.ItSystem");
            DropForeignKey("dbo.Technology", "ProgLanguage_Id", "dbo.ProgLanguage");
            DropForeignKey("dbo.Technology", "Id", "dbo.ItSystem");
            DropForeignKey("dbo.Technology", "Environment_Id", "dbo.Environment");
            DropForeignKey("dbo.Technology", "DatabaseType_Id", "dbo.DatabaseType");
            DropForeignKey("dbo.TaskSupport", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.SuperUser", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystem", "ParentItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystem", "Municipality_Id", "dbo.Municipality");
            DropForeignKey("dbo.KLE", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.KLE", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.Infrastructure", "Supplier_Id", "dbo.Supplier");
            DropForeignKey("dbo.Infrastructure", "Id", "dbo.ItSystem");
            DropForeignKey("dbo.Infrastructure", "Host_Id", "dbo.Host");
            DropForeignKey("dbo.Infrastructure", "Department_Id", "dbo.Department");
            DropForeignKey("dbo.Wish", "Interface_Id", "dbo.Interface");
            DropForeignKey("dbo.Interface", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.Wish", "Functionality_Id", "dbo.Functionality");
            DropForeignKey("dbo.Functionality", "Id", "dbo.ItSystem");
            DropForeignKey("dbo.ExtReference", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.ExtReference", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.ExtReference", "ExtReferenceType_Id", "dbo.ExtReferenceType");
            DropForeignKey("dbo.Component", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.BasicData", "ItSystem_Id", "dbo.ItSystem");
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
            DropIndex("dbo.UserRoles", new[] { "Role_Id" });
            DropIndex("dbo.UserRoles", new[] { "User_Id" });
            DropIndex("dbo.PasswordResetRequest", new[] { "User_Id" });
            DropIndex("dbo.User", new[] { "Municipality_Id" });
            DropIndex("dbo.Localization", new[] { "Municipality_Id" });
            DropIndex("dbo.Stakeholder", new[] { "ItProject_Id" });
            DropIndex("dbo.Risk", new[] { "ItProject_Id" });
            DropIndex("dbo.Resource", new[] { "ItProject_Id" });
            DropIndex("dbo.ItProject", new[] { "ProjectType_Id" });
            DropIndex("dbo.Milestone", new[] { "ProjectStatus_Id" });
            DropIndex("dbo.ProjectStatus", new[] { "Id" });
            DropIndex("dbo.ItProject", new[] { "ProjectCategory_Id" });
            DropIndex("dbo.PreAnalysis", new[] { "Id" });
            DropIndex("dbo.ItProject", new[] { "PartItProjectLeader_Id" });
            DropIndex("dbo.Organization", new[] { "Id" });
            DropIndex("dbo.ItProject", new[] { "Municipality_Id" });
            DropIndex("dbo.ItProject", new[] { "ItProjectOwner_Id" });
            DropIndex("dbo.ItProject", new[] { "ItProjectLeader_Id" });
            DropIndex("dbo.Hierarchy", new[] { "ItProjectRef_Id" });
            DropIndex("dbo.Hierarchy", new[] { "Id" });
            DropIndex("dbo.Hierarchy", new[] { "ItProgramRef_Id" });
            DropIndex("dbo.Handover", new[] { "Id" });
            DropIndex("dbo.GoalStatus", new[] { "Id" });
            DropIndex("dbo.Goal", new[] { "GoalStatus_Id" });
            DropIndex("dbo.Economy", new[] { "ItProject_Id" });
            DropIndex("dbo.ItProject", new[] { "Consultant_Id" });
            DropIndex("dbo.Person", new[] { "Municipality_Id" });
            DropIndex("dbo.ItSystem", new[] { "Person_Id" });
            DropIndex("dbo.UserAdministration", new[] { "Id" });
            DropIndex("dbo.Technology", new[] { "ProgLanguage_Id" });
            DropIndex("dbo.Technology", new[] { "Id" });
            DropIndex("dbo.Technology", new[] { "Environment_Id" });
            DropIndex("dbo.Technology", new[] { "DatabaseType_Id" });
            DropIndex("dbo.TaskSupport", new[] { "ItSystem_Id" });
            DropIndex("dbo.SuperUser", new[] { "ItSystem_Id" });
            DropIndex("dbo.ItSystem", new[] { "ParentItSystem_Id" });
            DropIndex("dbo.ItSystem", new[] { "Municipality_Id" });
            DropIndex("dbo.KLE", new[] { "ItSystem_Id" });
            DropIndex("dbo.KLE", new[] { "ItProject_Id" });
            DropIndex("dbo.Infrastructure", new[] { "Supplier_Id" });
            DropIndex("dbo.Infrastructure", new[] { "Id" });
            DropIndex("dbo.Infrastructure", new[] { "Host_Id" });
            DropIndex("dbo.Infrastructure", new[] { "Department_Id" });
            DropIndex("dbo.Wish", new[] { "Interface_Id" });
            DropIndex("dbo.Interface", new[] { "ItSystem_Id" });
            DropIndex("dbo.Wish", new[] { "Functionality_Id" });
            DropIndex("dbo.Functionality", new[] { "Id" });
            DropIndex("dbo.ExtReference", new[] { "ItSystem_Id" });
            DropIndex("dbo.ExtReference", new[] { "ItProject_Id" });
            DropIndex("dbo.ExtReference", new[] { "ExtReferenceType_Id" });
            DropIndex("dbo.Component", new[] { "ItSystem_Id" });
            DropIndex("dbo.BasicData", new[] { "ItSystem_Id" });
            DropIndex("dbo.Communication", new[] { "ItProject_Id" });
            DropIndex("dbo.Configuration", new[] { "Id" });
            DropIndex("dbo.ItContract", new[] { "ContractType_Id" });
            DropIndex("dbo.ItContract", new[] { "ContractTemplate_Id" });
            DropTable("dbo.UserRoles");
            DropTable("dbo.Text");
            DropTable("dbo.ShipNotice");
            DropTable("dbo.PurchaseForm");
            DropTable("dbo.PaymentModel");
            DropTable("dbo.Payment");
            DropTable("dbo.Role");
            DropTable("dbo.PasswordResetRequest");
            DropTable("dbo.User");
            DropTable("dbo.Localization");
            DropTable("dbo.Stakeholder");
            DropTable("dbo.Risk");
            DropTable("dbo.Resource");
            DropTable("dbo.ProjectType");
            DropTable("dbo.Milestone");
            DropTable("dbo.ProjectStatus");
            DropTable("dbo.ProjectCategory");
            DropTable("dbo.PreAnalysis");
            DropTable("dbo.Organization");
            DropTable("dbo.Hierarchy");
            DropTable("dbo.Handover");
            DropTable("dbo.Goal");
            DropTable("dbo.GoalStatus");
            DropTable("dbo.Economy");
            DropTable("dbo.UserAdministration");
            DropTable("dbo.ProgLanguage");
            DropTable("dbo.Environment");
            DropTable("dbo.DatabaseType");
            DropTable("dbo.Technology");
            DropTable("dbo.TaskSupport");
            DropTable("dbo.SuperUser");
            DropTable("dbo.KLE");
            DropTable("dbo.Supplier");
            DropTable("dbo.Host");
            DropTable("dbo.Department");
            DropTable("dbo.Infrastructure");
            DropTable("dbo.Interface");
            DropTable("dbo.Wish");
            DropTable("dbo.Functionality");
            DropTable("dbo.ExtReferenceType");
            DropTable("dbo.ExtReference");
            DropTable("dbo.Component");
            DropTable("dbo.BasicData");
            DropTable("dbo.ItSystem");
            DropTable("dbo.Person");
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
