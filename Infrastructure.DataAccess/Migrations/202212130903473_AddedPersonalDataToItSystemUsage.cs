namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPersonalDataToItSystemUsage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItSystemUsagePersonalDatas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemUsageId = c.Int(nullable: false),
                        PersonalData = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsageId, cascadeDelete: true)
                .Index(t => t.ItSystemUsageId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystemUsagePersonalDatas", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropIndex("dbo.ItSystemUsagePersonalDatas", new[] { "ItSystemUsageId" });
            DropTable("dbo.ItSystemUsagePersonalDatas");
        }
    }
}
