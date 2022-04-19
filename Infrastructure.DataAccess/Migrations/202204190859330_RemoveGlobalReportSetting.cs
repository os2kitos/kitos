namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveGlobalReportSetting : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.GlobalConfigs", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.GlobalConfigs", "ObjectOwnerId", "dbo.User");
            DropIndex("dbo.GlobalConfigs", new[] { "ObjectOwnerId" });
            DropIndex("dbo.GlobalConfigs", new[] { "LastChangedByUserId" });
            DropTable("dbo.GlobalConfigs");
        }
        
        public override void Down()
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
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.GlobalConfigs", "LastChangedByUserId");
            CreateIndex("dbo.GlobalConfigs", "ObjectOwnerId");
            AddForeignKey("dbo.GlobalConfigs", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.GlobalConfigs", "LastChangedByUserId", "dbo.User", "Id");
        }
    }
}
