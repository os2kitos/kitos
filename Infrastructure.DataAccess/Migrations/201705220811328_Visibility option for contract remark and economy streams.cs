namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Visibilityoptionforcontractremarkandeconomystreams : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItContractRemarks",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        AccessModifier = c.Int(nullable: false),
                        Remark = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.ItContract", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            AddColumn("dbo.EconomyStream", "AccessModifier", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItContractRemarks", "Id", "dbo.ItContract");
            DropForeignKey("dbo.ItContractRemarks", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItContractRemarks", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.ItContractRemarks", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItContractRemarks", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItContractRemarks", new[] { "Id" });
            DropColumn("dbo.EconomyStream", "AccessModifier");
            DropTable("dbo.ItContractRemarks");
        }
    }
}
