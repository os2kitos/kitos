namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedGlobalConfig : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GlobalConfigs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        key = c.String(),
                        value = c.String(),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GlobalConfigs", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.GlobalConfigs", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.GlobalConfigs", new[] { "LastChangedByUserId" });
            DropIndex("dbo.GlobalConfigs", new[] { "ObjectOwnerId" });
            DropTable("dbo.GlobalConfigs");
        }
    }
}
