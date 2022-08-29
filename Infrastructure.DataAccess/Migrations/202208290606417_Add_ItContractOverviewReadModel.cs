namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_ItContractOverviewReadModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItContractOverviewReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrganizationId = c.Int(nullable: false),
                        SourceEntityId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .ForeignKey("dbo.ItContract", t => t.SourceEntityId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.SourceEntityId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItContractOverviewReadModels", "SourceEntityId", "dbo.ItContract");
            DropForeignKey("dbo.ItContractOverviewReadModels", "OrganizationId", "dbo.Organization");
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "SourceEntityId" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "OrganizationId" });
            DropTable("dbo.ItContractOverviewReadModels");
        }
    }
}
