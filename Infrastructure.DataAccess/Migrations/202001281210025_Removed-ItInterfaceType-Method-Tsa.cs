namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedItInterfaceTypeMethodTsa : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItInterfaceTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItInterfaceTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItInterface", "InterfaceTypeId", "dbo.ItInterfaceTypes");
            DropForeignKey("dbo.MethodTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.MethodTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItInterface", "MethodId", "dbo.MethodTypes");
            DropForeignKey("dbo.TsaTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.TsaTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItInterface", "TsaId", "dbo.TsaTypes");
            DropForeignKey("dbo.LocalItInterfaceTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalItInterfaceTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalItInterfaceTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalMethodTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalMethodTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalMethodTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalTsaTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalTsaTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalTsaTypes", "OrganizationId", "dbo.Organization");
            DropIndex("dbo.ItInterface", new[] { "InterfaceTypeId" });
            DropIndex("dbo.ItInterface", new[] { "TsaId" });
            DropIndex("dbo.ItInterface", new[] { "MethodId" });
            DropIndex("dbo.ItInterfaceTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItInterfaceTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.MethodTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.MethodTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.TsaTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.TsaTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalItInterfaceTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalItInterfaceTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalItInterfaceTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalMethodTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalMethodTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalMethodTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalTsaTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalTsaTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalTsaTypes", new[] { "LastChangedByUserId" });
            DropTable("dbo.ItInterfaceTypes");
            DropTable("dbo.MethodTypes");
            DropTable("dbo.TsaTypes");
            DropTable("dbo.LocalItInterfaceTypes");
            DropTable("dbo.LocalMethodTypes");
            DropTable("dbo.LocalTsaTypes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.LocalTsaTypes",
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
                "dbo.LocalMethodTypes",
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
                "dbo.LocalItInterfaceTypes",
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
                "dbo.TsaTypes",
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
            
            CreateTable(
                "dbo.MethodTypes",
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
            
            CreateTable(
                "dbo.ItInterfaceTypes",
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
            
            CreateIndex("dbo.LocalTsaTypes", "LastChangedByUserId");
            CreateIndex("dbo.LocalTsaTypes", "ObjectOwnerId");
            CreateIndex("dbo.LocalTsaTypes", "OrganizationId");
            CreateIndex("dbo.LocalMethodTypes", "LastChangedByUserId");
            CreateIndex("dbo.LocalMethodTypes", "ObjectOwnerId");
            CreateIndex("dbo.LocalMethodTypes", "OrganizationId");
            CreateIndex("dbo.LocalItInterfaceTypes", "LastChangedByUserId");
            CreateIndex("dbo.LocalItInterfaceTypes", "ObjectOwnerId");
            CreateIndex("dbo.LocalItInterfaceTypes", "OrganizationId");
            CreateIndex("dbo.TsaTypes", "LastChangedByUserId");
            CreateIndex("dbo.TsaTypes", "ObjectOwnerId");
            CreateIndex("dbo.MethodTypes", "LastChangedByUserId");
            CreateIndex("dbo.MethodTypes", "ObjectOwnerId");
            CreateIndex("dbo.ItInterfaceTypes", "LastChangedByUserId");
            CreateIndex("dbo.ItInterfaceTypes", "ObjectOwnerId");
            CreateIndex("dbo.ItInterface", "MethodId");
            CreateIndex("dbo.ItInterface", "TsaId");
            CreateIndex("dbo.ItInterface", "InterfaceTypeId");
            AddForeignKey("dbo.LocalTsaTypes", "OrganizationId", "dbo.Organization", "Id", cascadeDelete: true);
            AddForeignKey("dbo.LocalTsaTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.LocalTsaTypes", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.LocalMethodTypes", "OrganizationId", "dbo.Organization", "Id", cascadeDelete: true);
            AddForeignKey("dbo.LocalMethodTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.LocalMethodTypes", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.LocalItInterfaceTypes", "OrganizationId", "dbo.Organization", "Id", cascadeDelete: true);
            AddForeignKey("dbo.LocalItInterfaceTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.LocalItInterfaceTypes", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItInterface", "TsaId", "dbo.TsaTypes", "Id");
            AddForeignKey("dbo.TsaTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.TsaTypes", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItInterface", "MethodId", "dbo.MethodTypes", "Id");
            AddForeignKey("dbo.MethodTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.MethodTypes", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItInterface", "InterfaceTypeId", "dbo.ItInterfaceTypes", "Id");
            AddForeignKey("dbo.ItInterfaceTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.ItInterfaceTypes", "LastChangedByUserId", "dbo.User", "Id");
        }
    }
}
