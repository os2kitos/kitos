namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Removed_AppTypes_DataLevel_And_GDPR_From_System : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItSystemTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItSystem", "AppTypeOptionId", "dbo.ItSystemTypes");
            DropForeignKey("dbo.ItSystemDataWorkerRelations", "DataWorkerId", "dbo.Organization");
            DropForeignKey("dbo.ItSystemDataWorkerRelations", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemDataWorkerRelations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItSystemDataWorkerRelations", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.LocalItSystemTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalItSystemTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalItSystemTypes", "OrganizationId", "dbo.Organization");
            DropIndex("dbo.ItSystem", new[] { "AppTypeOptionId" });
            DropIndex("dbo.ItSystemTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItSystemDataWorkerRelations", new[] { "ItSystemId" });
            DropIndex("dbo.ItSystemDataWorkerRelations", new[] { "DataWorkerId" });
            DropIndex("dbo.ItSystemDataWorkerRelations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemDataWorkerRelations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalItSystemTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalItSystemTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalItSystemTypes", new[] { "LastChangedByUserId" });
            DropColumn("dbo.ItSystem", "AppTypeOptionId");
            DropColumn("dbo.ItSystem", "GeneralPurpose");
            DropColumn("dbo.ItSystem", "DataLevel");
            DropColumn("dbo.ItSystem", "ContainsLegalInfo");
            DropColumn("dbo.ItSystem", "IsDataTransferedToThirdCountries");
            DropColumn("dbo.ItSystem", "DataIsTransferedTo");
            DropTable("dbo.ItSystemTypes");
            DropTable("dbo.ItSystemDataWorkerRelations");
            DropTable("dbo.LocalItSystemTypes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.LocalItSystemTypes",
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
                "dbo.ItSystemDataWorkerRelations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemId = c.Int(nullable: false),
                        DataWorkerId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItSystemTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsLocallyAvailable = c.Boolean(nullable: false),
                        IsObligatory = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Description = c.String(),
                        IsEnabled = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ItSystem", "DataIsTransferedTo", c => c.String());
            AddColumn("dbo.ItSystem", "IsDataTransferedToThirdCountries", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystem", "ContainsLegalInfo", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystem", "DataLevel", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystem", "GeneralPurpose", c => c.String());
            AddColumn("dbo.ItSystem", "AppTypeOptionId", c => c.Int());
            CreateIndex("dbo.LocalItSystemTypes", "LastChangedByUserId");
            CreateIndex("dbo.LocalItSystemTypes", "ObjectOwnerId");
            CreateIndex("dbo.LocalItSystemTypes", "OrganizationId");
            CreateIndex("dbo.ItSystemDataWorkerRelations", "LastChangedByUserId");
            CreateIndex("dbo.ItSystemDataWorkerRelations", "ObjectOwnerId");
            CreateIndex("dbo.ItSystemDataWorkerRelations", "DataWorkerId");
            CreateIndex("dbo.ItSystemDataWorkerRelations", "ItSystemId");
            CreateIndex("dbo.ItSystemTypes", "LastChangedByUserId");
            CreateIndex("dbo.ItSystemTypes", "ObjectOwnerId");
            CreateIndex("dbo.ItSystem", "AppTypeOptionId");
            AddForeignKey("dbo.LocalItSystemTypes", "OrganizationId", "dbo.Organization", "Id", cascadeDelete: true);
            AddForeignKey("dbo.LocalItSystemTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.LocalItSystemTypes", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItSystemDataWorkerRelations", "ItSystemId", "dbo.ItSystem", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItSystemDataWorkerRelations", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.ItSystemDataWorkerRelations", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItSystemDataWorkerRelations", "DataWorkerId", "dbo.Organization", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItSystem", "AppTypeOptionId", "dbo.ItSystemTypes", "Id");
            AddForeignKey("dbo.ItSystemTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.ItSystemTypes", "LastChangedByUserId", "dbo.User", "Id");
        }
    }
}
