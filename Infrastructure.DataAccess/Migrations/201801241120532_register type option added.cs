namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class registertypeoptionadded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LocalRegisterTypes",
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.RegisterTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        IsLocallyAvailable = c.Boolean(nullable: false),
                        IsObligatory = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Description = c.String(),
                        IsEnabled = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            AddColumn("dbo.ItSystemUsage", "RegisterType_Id", c => c.Int());
            CreateIndex("dbo.ItSystemUsage", "RegisterType_Id");
            AddForeignKey("dbo.ItSystemUsage", "RegisterType_Id", "dbo.RegisterTypes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystemUsage", "RegisterType_Id", "dbo.RegisterTypes");
            DropForeignKey("dbo.RegisterTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.RegisterTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalRegisterTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalRegisterTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalRegisterTypes", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.RegisterTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.RegisterTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalRegisterTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalRegisterTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalRegisterTypes", new[] { "OrganizationId" });
            DropIndex("dbo.ItSystemUsage", new[] { "RegisterType_Id" });
            DropColumn("dbo.ItSystemUsage", "RegisterType_Id");
            DropTable("dbo.RegisterTypes");
            DropTable("dbo.LocalRegisterTypes");
        }
    }
}
