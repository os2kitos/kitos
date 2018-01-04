namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class localobjectsadded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LocalRegularPersonalDataTypes",
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
                "dbo.LocalSensitivePersonalDataTypes",
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LocalSensitivePersonalDataTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalSensitivePersonalDataTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalSensitivePersonalDataTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalRegularPersonalDataTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalRegularPersonalDataTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalRegularPersonalDataTypes", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.LocalSensitivePersonalDataTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalSensitivePersonalDataTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalSensitivePersonalDataTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalRegularPersonalDataTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalRegularPersonalDataTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalRegularPersonalDataTypes", new[] { "OrganizationId" });
            DropTable("dbo.LocalSensitivePersonalDataTypes");
            DropTable("dbo.LocalRegularPersonalDataTypes");
        }
    }
}
