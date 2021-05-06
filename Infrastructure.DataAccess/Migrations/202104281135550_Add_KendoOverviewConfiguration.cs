namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_KendoOverviewConfiguration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.KendoOrganizationalConfigurations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OverviewType = c.Int(nullable: false),
                        Configuration = c.String(nullable: false),
                        OrganizationId = c.Int(nullable: false),
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
            DropForeignKey("dbo.KendoOrganizationalConfigurations", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.KendoOrganizationalConfigurations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.KendoOrganizationalConfigurations", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.KendoOrganizationalConfigurations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.KendoOrganizationalConfigurations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.KendoOrganizationalConfigurations", new[] { "OrganizationId" });
            DropTable("dbo.KendoOrganizationalConfigurations");
        }
    }
}
