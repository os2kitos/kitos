namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addedrestofthelocaloptions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LocalAgreementElementTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalArchiveTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalDataTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalFrequencyTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalGoalTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalHandoverTrialTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalInterfaceTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalItContractRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalItContractTemplateTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalItContractTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalItProjectRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalItProjectTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalItSystemTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalMethodTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalOptionExtendTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalPaymentFreqencyTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalPaymentModelTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalPriceRegulationTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalProcurementStrategyTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalPurchaseFormTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalReportCategoryTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalSensitiveDataTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalTerminationDeadlineTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalTsaTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            AddColumn("dbo.ItContractRoles", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.BusinessTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.FrequencyTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ArchiveTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.AgreementElementTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTemplateTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.GoalTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectRoles", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.OrganizationUnitRoles", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.HandoverTrialTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.OptionExtendTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentFreqencyTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentModelTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.PriceRegulationTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProcurementStrategyTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.PurchaseFormTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.TerminationDeadlineTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemRoles", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.SensitiveDataTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.InterfaceTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItInterfaceTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.MethodTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.TsaTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ReportCategoryTypes", "IsDisabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LocalTsaTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalTsaTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalTsaTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalTerminationDeadlineTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalTerminationDeadlineTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalTerminationDeadlineTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalSensitiveDataTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalSensitiveDataTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalSensitiveDataTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalReportCategoryTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalReportCategoryTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalReportCategoryTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalPurchaseFormTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalPurchaseFormTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalPurchaseFormTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalProcurementStrategyTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalProcurementStrategyTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalProcurementStrategyTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalPriceRegulationTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalPriceRegulationTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalPriceRegulationTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalPaymentModelTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalPaymentModelTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalPaymentModelTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalPaymentFreqencyTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalPaymentFreqencyTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalPaymentFreqencyTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalOptionExtendTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalOptionExtendTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalOptionExtendTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalMethodTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalMethodTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalMethodTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalItSystemTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalItSystemTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalItSystemTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalItProjectTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalItProjectTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalItProjectTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalItProjectRoles", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalItProjectRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalItProjectRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalItContractTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalItContractTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalItContractTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalItContractTemplateTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalItContractTemplateTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalItContractTemplateTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalItContractRoles", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalItContractRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalItContractRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalInterfaceTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalInterfaceTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalInterfaceTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalHandoverTrialTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalHandoverTrialTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalHandoverTrialTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalGoalTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalGoalTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalGoalTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalFrequencyTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalFrequencyTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalFrequencyTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalDataTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalDataTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalDataTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalArchiveTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalArchiveTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalArchiveTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalAgreementElementTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalAgreementElementTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalAgreementElementTypes", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.LocalTsaTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalTsaTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalTsaTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalTerminationDeadlineTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalTerminationDeadlineTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalTerminationDeadlineTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalSensitiveDataTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalSensitiveDataTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalSensitiveDataTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalReportCategoryTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalReportCategoryTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalReportCategoryTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalPurchaseFormTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalPurchaseFormTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalPurchaseFormTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalProcurementStrategyTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalProcurementStrategyTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalProcurementStrategyTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalPriceRegulationTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalPriceRegulationTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalPriceRegulationTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalPaymentModelTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalPaymentModelTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalPaymentModelTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalPaymentFreqencyTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalPaymentFreqencyTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalPaymentFreqencyTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalOptionExtendTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalOptionExtendTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalOptionExtendTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalMethodTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalMethodTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalMethodTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalItSystemTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalItSystemTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalItSystemTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalItProjectTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalItProjectTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalItProjectTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalItProjectRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalItProjectRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalItProjectRoles", new[] { "OrganizationId" });
            DropIndex("dbo.LocalItContractTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalItContractTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalItContractTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalItContractTemplateTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalItContractTemplateTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalItContractTemplateTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalItContractRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalItContractRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalItContractRoles", new[] { "OrganizationId" });
            DropIndex("dbo.LocalInterfaceTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalInterfaceTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalInterfaceTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalHandoverTrialTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalHandoverTrialTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalHandoverTrialTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalGoalTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalGoalTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalGoalTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalFrequencyTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalFrequencyTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalFrequencyTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalDataTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalDataTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalDataTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalArchiveTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalArchiveTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalArchiveTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalAgreementElementTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalAgreementElementTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalAgreementElementTypes", new[] { "OrganizationId" });
            DropColumn("dbo.ReportCategoryTypes", "IsDisabled");
            DropColumn("dbo.TsaTypes", "IsDisabled");
            DropColumn("dbo.MethodTypes", "IsDisabled");
            DropColumn("dbo.ItInterfaceTypes", "IsDisabled");
            DropColumn("dbo.InterfaceTypes", "IsDisabled");
            DropColumn("dbo.SensitiveDataTypes", "IsDisabled");
            DropColumn("dbo.ItSystemRoles", "IsDisabled");
            DropColumn("dbo.TerminationDeadlineTypes", "IsDisabled");
            DropColumn("dbo.PurchaseFormTypes", "IsDisabled");
            DropColumn("dbo.ProcurementStrategyTypes", "IsDisabled");
            DropColumn("dbo.PriceRegulationTypes", "IsDisabled");
            DropColumn("dbo.PaymentModelTypes", "IsDisabled");
            DropColumn("dbo.PaymentFreqencyTypes", "IsDisabled");
            DropColumn("dbo.OptionExtendTypes", "IsDisabled");
            DropColumn("dbo.HandoverTrialTypes", "IsDisabled");
            DropColumn("dbo.OrganizationUnitRoles", "IsDisabled");
            DropColumn("dbo.ItProjectRoles", "IsDisabled");
            DropColumn("dbo.ItProjectTypes", "IsDisabled");
            DropColumn("dbo.GoalTypes", "IsDisabled");
            DropColumn("dbo.ItContractTypes", "IsDisabled");
            DropColumn("dbo.ItContractTemplateTypes", "IsDisabled");
            DropColumn("dbo.AgreementElementTypes", "IsDisabled");
            DropColumn("dbo.ArchiveTypes", "IsDisabled");
            DropColumn("dbo.FrequencyTypes", "IsDisabled");
            DropColumn("dbo.DataTypes", "IsDisabled");
            DropColumn("dbo.BusinessTypes", "IsDisabled");
            DropColumn("dbo.ItSystemTypes", "IsDisabled");
            DropColumn("dbo.ItContractRoles", "IsDisabled");
            DropTable("dbo.LocalTsaTypes");
            DropTable("dbo.LocalTerminationDeadlineTypes");
            DropTable("dbo.LocalSensitiveDataTypes");
            DropTable("dbo.LocalReportCategoryTypes");
            DropTable("dbo.LocalPurchaseFormTypes");
            DropTable("dbo.LocalProcurementStrategyTypes");
            DropTable("dbo.LocalPriceRegulationTypes");
            DropTable("dbo.LocalPaymentModelTypes");
            DropTable("dbo.LocalPaymentFreqencyTypes");
            DropTable("dbo.LocalOptionExtendTypes");
            DropTable("dbo.LocalMethodTypes");
            DropTable("dbo.LocalItSystemTypes");
            DropTable("dbo.LocalItProjectTypes");
            DropTable("dbo.LocalItProjectRoles");
            DropTable("dbo.LocalItContractTypes");
            DropTable("dbo.LocalItContractTemplateTypes");
            DropTable("dbo.LocalItContractRoles");
            DropTable("dbo.LocalInterfaceTypes");
            DropTable("dbo.LocalHandoverTrialTypes");
            DropTable("dbo.LocalGoalTypes");
            DropTable("dbo.LocalFrequencyTypes");
            DropTable("dbo.LocalDataTypes");
            DropTable("dbo.LocalArchiveTypes");
            DropTable("dbo.LocalAgreementElementTypes");
        }
    }
}
