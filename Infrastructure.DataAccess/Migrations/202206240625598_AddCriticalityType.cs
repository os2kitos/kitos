namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCriticalityType : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CriticalityTypes",
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.Uuid, unique: true, name: "UX_Option_Uuid")
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalCriticalityTypes",
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
            
            AddColumn("dbo.ItContract", "CriticalityTypeId", c => c.Int());
            CreateIndex("dbo.ItContract", "CriticalityTypeId");
            AddForeignKey("dbo.ItContract", "CriticalityTypeId", "dbo.CriticalityTypes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LocalCriticalityTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalCriticalityTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalCriticalityTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "CriticalityTypeId", "dbo.CriticalityTypes");
            DropForeignKey("dbo.CriticalityTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.CriticalityTypes", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.LocalCriticalityTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalCriticalityTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalCriticalityTypes", new[] { "OrganizationId" });
            DropIndex("dbo.CriticalityTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.CriticalityTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.CriticalityTypes", "UX_Option_Uuid");
            DropIndex("dbo.ItContract", new[] { "CriticalityTypeId" });
            DropColumn("dbo.ItContract", "CriticalityTypeId");
            DropTable("dbo.LocalCriticalityTypes");
            DropTable("dbo.CriticalityTypes");
        }
    }
}
