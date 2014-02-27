namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Localization", "Municipality_Id", "dbo.Municipality");
            DropForeignKey("dbo.User", "Municipality_Id", "dbo.Municipality");
            DropIndex("dbo.Localization", new[] { "Municipality_Id" });
            DropIndex("dbo.User", new[] { "Municipality_Id" });
            CreateTable(
                "dbo.Method",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InterfaceType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProtocolType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SystemType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
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
                "dbo.ProjectPhaseLocale",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Municipality", t => t.Id)
                .ForeignKey("dbo.ProjectPhase", t => t.Id)
                .Index(t => t.Id)
                .Index(t => t.Id);
            
            AddColumn("dbo.ContractTemplate", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ContractTemplate", "Note", c => c.String(unicode: false));
            AddColumn("dbo.ContractType", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ContractType", "Note", c => c.String(unicode: false));
            AddColumn("dbo.Configuration", "ItSupportGuide", c => c.String(unicode: false));
            AddColumn("dbo.Configuration", "ShowTabOverview", c => c.Boolean(nullable: false));
            AddColumn("dbo.Configuration", "ShowColumnUsage", c => c.Boolean(nullable: false));
            AddColumn("dbo.Configuration", "ShowColumnMandatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.Configuration", "ItSystemGuide", c => c.String(unicode: false));
            AddColumn("dbo.Configuration", "ShowRightOfUse", c => c.Boolean(nullable: false));
            AddColumn("dbo.Configuration", "ShowLicense", c => c.Boolean(nullable: false));
            AddColumn("dbo.Configuration", "ShowOperation", c => c.Boolean(nullable: false));
            AddColumn("dbo.Configuration", "ShowMaintenance", c => c.Boolean(nullable: false));
            AddColumn("dbo.Configuration", "ShowSupport", c => c.Boolean(nullable: false));
            AddColumn("dbo.Configuration", "ShowServerLicense", c => c.Boolean(nullable: false));
            AddColumn("dbo.Configuration", "ShowServerOperation", c => c.Boolean(nullable: false));
            AddColumn("dbo.Configuration", "ShowBackup", c => c.Boolean(nullable: false));
            AddColumn("dbo.Configuration", "ShowSurveillance", c => c.Boolean(nullable: false));
            AddColumn("dbo.Configuration", "ShowOther", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystem", "SystemType_Id", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystem", "InterfaceType_Id", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystem", "ProtocolType_Id", c => c.Int(nullable: false));
            AddColumn("dbo.Interface", "Method_Id", c => c.Int(nullable: false));
            AddColumn("dbo.DatabaseType", "Name", c => c.String(unicode: false));
            AddColumn("dbo.DatabaseType", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.DatabaseType", "Note", c => c.String(unicode: false));
            AddColumn("dbo.Environment", "Name", c => c.String(unicode: false));
            AddColumn("dbo.Environment", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Environment", "Note", c => c.String(unicode: false));
            AddColumn("dbo.ProjectCategory", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProjectCategory", "Note", c => c.String(unicode: false));
            AddColumn("dbo.ProjectStatus", "ProjectPhase_Id", c => c.Int());
            AddColumn("dbo.ProjectType", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProjectType", "Note", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "EsdhRef", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "CmdbRef", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "IdmRef", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "FolderRef", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "ItProject", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "ItProgram", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "FocusArea", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "Fase1", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "Fase2", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "Fase3", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "Fase4", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "Fase5", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "RightOfUse", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "License", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "Operation", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "Maintenance", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "Support", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "ServerLicense", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "ServerOperation", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "Backup", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "Surveillance", c => c.String(unicode: false));
            AddColumn("dbo.Localization", "Other", c => c.String(unicode: false));
            AddColumn("dbo.PaymentModel", "Name", c => c.String(unicode: false));
            AddColumn("dbo.PaymentModel", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentModel", "Note", c => c.String(unicode: false));
            AddColumn("dbo.PurchaseForm", "Name", c => c.String(unicode: false));
            AddColumn("dbo.PurchaseForm", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.PurchaseForm", "Note", c => c.String(unicode: false));
            AlterColumn("dbo.User", "Municipality_Id", c => c.Int());
            CreateIndex("dbo.Interface", "Method_Id");
            CreateIndex("dbo.ItSystem", "InterfaceType_Id");
            CreateIndex("dbo.ItSystem", "ProtocolType_Id");
            CreateIndex("dbo.ItSystem", "SystemType_Id");
            CreateIndex("dbo.ProjectStatus", "ProjectPhase_Id");
            CreateIndex("dbo.User", "Municipality_Id");
            AddForeignKey("dbo.Interface", "Method_Id", "dbo.Method", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItSystem", "InterfaceType_Id", "dbo.InterfaceType", "Id");
            AddForeignKey("dbo.ItSystem", "ProtocolType_Id", "dbo.ProtocolType", "Id");
            AddForeignKey("dbo.ItSystem", "SystemType_Id", "dbo.SystemType", "Id");
            AddForeignKey("dbo.ProjectStatus", "ProjectPhase_Id", "dbo.ProjectPhase", "Id");
            AddForeignKey("dbo.User", "Municipality_Id", "dbo.Municipality", "Id");
            DropColumn("dbo.Configuration", "EsdhRef");
            DropColumn("dbo.Configuration", "CmdbRef");
            DropColumn("dbo.Configuration", "FolderRef");
            DropColumn("dbo.Configuration", "ItProject");
            DropColumn("dbo.Configuration", "ItProgram");
            DropColumn("dbo.Configuration", "FocusArea");
            DropColumn("dbo.Configuration", "Fase1");
            DropColumn("dbo.Configuration", "Fase2");
            DropColumn("dbo.Configuration", "Fase3");
            DropColumn("dbo.Configuration", "Fase4");
            DropColumn("dbo.Configuration", "Fase5");
            DropColumn("dbo.Localization", "Literal");
            DropColumn("dbo.Localization", "Value");
            DropColumn("dbo.Localization", "Municipality_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Localization", "Municipality_Id", c => c.Int(nullable: false));
            AddColumn("dbo.Localization", "Value", c => c.String(nullable: false, unicode: false));
            AddColumn("dbo.Localization", "Literal", c => c.String(nullable: false, unicode: false));
            AddColumn("dbo.Configuration", "Fase5", c => c.String(unicode: false));
            AddColumn("dbo.Configuration", "Fase4", c => c.String(unicode: false));
            AddColumn("dbo.Configuration", "Fase3", c => c.String(unicode: false));
            AddColumn("dbo.Configuration", "Fase2", c => c.String(unicode: false));
            AddColumn("dbo.Configuration", "Fase1", c => c.String(unicode: false));
            AddColumn("dbo.Configuration", "FocusArea", c => c.String(unicode: false));
            AddColumn("dbo.Configuration", "ItProgram", c => c.String(unicode: false));
            AddColumn("dbo.Configuration", "ItProject", c => c.String(unicode: false));
            AddColumn("dbo.Configuration", "FolderRef", c => c.String(unicode: false));
            AddColumn("dbo.Configuration", "CmdbRef", c => c.String(unicode: false));
            AddColumn("dbo.Configuration", "EsdhRef", c => c.String(unicode: false));
            DropForeignKey("dbo.User", "Municipality_Id", "dbo.Municipality");
            DropForeignKey("dbo.ProjectStatus", "ProjectPhase_Id", "dbo.ProjectPhase");
            DropForeignKey("dbo.ProjectPhaseLocale", "Id", "dbo.ProjectPhase");
            DropForeignKey("dbo.ProjectPhaseLocale", "Id", "dbo.Municipality");
            DropForeignKey("dbo.ItSystem", "SystemType_Id", "dbo.SystemType");
            DropForeignKey("dbo.ItSystem", "ProtocolType_Id", "dbo.ProtocolType");
            DropForeignKey("dbo.ItSystem", "InterfaceType_Id", "dbo.InterfaceType");
            DropForeignKey("dbo.Interface", "Method_Id", "dbo.Method");
            DropIndex("dbo.User", new[] { "Municipality_Id" });
            DropIndex("dbo.ProjectStatus", new[] { "ProjectPhase_Id" });
            DropIndex("dbo.ProjectPhaseLocale", new[] { "Id" });
            DropIndex("dbo.ProjectPhaseLocale", new[] { "Id" });
            DropIndex("dbo.ItSystem", new[] { "SystemType_Id" });
            DropIndex("dbo.ItSystem", new[] { "ProtocolType_Id" });
            DropIndex("dbo.ItSystem", new[] { "InterfaceType_Id" });
            DropIndex("dbo.Interface", new[] { "Method_Id" });
            AlterColumn("dbo.User", "Municipality_Id", c => c.Int(nullable: false));
            DropColumn("dbo.PurchaseForm", "Note");
            DropColumn("dbo.PurchaseForm", "IsActive");
            DropColumn("dbo.PurchaseForm", "Name");
            DropColumn("dbo.PaymentModel", "Note");
            DropColumn("dbo.PaymentModel", "IsActive");
            DropColumn("dbo.PaymentModel", "Name");
            DropColumn("dbo.Localization", "Other");
            DropColumn("dbo.Localization", "Surveillance");
            DropColumn("dbo.Localization", "Backup");
            DropColumn("dbo.Localization", "ServerOperation");
            DropColumn("dbo.Localization", "ServerLicense");
            DropColumn("dbo.Localization", "Support");
            DropColumn("dbo.Localization", "Maintenance");
            DropColumn("dbo.Localization", "Operation");
            DropColumn("dbo.Localization", "License");
            DropColumn("dbo.Localization", "RightOfUse");
            DropColumn("dbo.Localization", "Fase5");
            DropColumn("dbo.Localization", "Fase4");
            DropColumn("dbo.Localization", "Fase3");
            DropColumn("dbo.Localization", "Fase2");
            DropColumn("dbo.Localization", "Fase1");
            DropColumn("dbo.Localization", "FocusArea");
            DropColumn("dbo.Localization", "ItProgram");
            DropColumn("dbo.Localization", "ItProject");
            DropColumn("dbo.Localization", "FolderRef");
            DropColumn("dbo.Localization", "IdmRef");
            DropColumn("dbo.Localization", "CmdbRef");
            DropColumn("dbo.Localization", "EsdhRef");
            DropColumn("dbo.ProjectType", "Note");
            DropColumn("dbo.ProjectType", "IsActive");
            DropColumn("dbo.ProjectStatus", "ProjectPhase_Id");
            DropColumn("dbo.ProjectCategory", "Note");
            DropColumn("dbo.ProjectCategory", "IsActive");
            DropColumn("dbo.Environment", "Note");
            DropColumn("dbo.Environment", "IsActive");
            DropColumn("dbo.Environment", "Name");
            DropColumn("dbo.DatabaseType", "Note");
            DropColumn("dbo.DatabaseType", "IsActive");
            DropColumn("dbo.DatabaseType", "Name");
            DropColumn("dbo.Interface", "Method_Id");
            DropColumn("dbo.ItSystem", "ProtocolType_Id");
            DropColumn("dbo.ItSystem", "InterfaceType_Id");
            DropColumn("dbo.ItSystem", "SystemType_Id");
            DropColumn("dbo.Configuration", "ShowOther");
            DropColumn("dbo.Configuration", "ShowSurveillance");
            DropColumn("dbo.Configuration", "ShowBackup");
            DropColumn("dbo.Configuration", "ShowServerOperation");
            DropColumn("dbo.Configuration", "ShowServerLicense");
            DropColumn("dbo.Configuration", "ShowSupport");
            DropColumn("dbo.Configuration", "ShowMaintenance");
            DropColumn("dbo.Configuration", "ShowOperation");
            DropColumn("dbo.Configuration", "ShowLicense");
            DropColumn("dbo.Configuration", "ShowRightOfUse");
            DropColumn("dbo.Configuration", "ItSystemGuide");
            DropColumn("dbo.Configuration", "ShowColumnMandatory");
            DropColumn("dbo.Configuration", "ShowColumnUsage");
            DropColumn("dbo.Configuration", "ShowTabOverview");
            DropColumn("dbo.Configuration", "ItSupportGuide");
            DropColumn("dbo.ContractType", "Note");
            DropColumn("dbo.ContractType", "IsActive");
            DropColumn("dbo.ContractTemplate", "Note");
            DropColumn("dbo.ContractTemplate", "IsActive");
            DropTable("dbo.ProjectPhaseLocale");
            DropTable("dbo.ProjectPhase");
            DropTable("dbo.SystemType");
            DropTable("dbo.ProtocolType");
            DropTable("dbo.InterfaceType");
            DropTable("dbo.Method");
            CreateIndex("dbo.User", "Municipality_Id");
            CreateIndex("dbo.Localization", "Municipality_Id");
            AddForeignKey("dbo.User", "Municipality_Id", "dbo.Municipality", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Localization", "Municipality_Id", "dbo.Municipality", "Id", cascadeDelete: true);
        }
    }
}
