namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedtaskrefOptOut : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TaskRefItSystemUsageOptOut",
                c => new
                    {
                        TaskRef_Id = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TaskRef_Id, t.ItSystemUsage_Id })
                .ForeignKey("dbo.TaskRef", t => t.TaskRef_Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .Index(t => t.TaskRef_Id)
                .Index(t => t.ItSystemUsage_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TaskRefItSystemUsageOptOut", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.TaskRefItSystemUsageOptOut", "TaskRef_Id", "dbo.TaskRef");
            DropIndex("dbo.TaskRefItSystemUsageOptOut", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.TaskRefItSystemUsageOptOut", new[] { "TaskRef_Id" });
            DropTable("dbo.TaskRefItSystemUsageOptOut");
        }
    }
}
