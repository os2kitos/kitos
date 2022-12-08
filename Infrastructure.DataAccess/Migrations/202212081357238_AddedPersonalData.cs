namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPersonalData : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItSystemUsagePersonalDatas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonalData = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .Index(t => t.ItSystemUsage_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystemUsagePersonalDatas", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropIndex("dbo.ItSystemUsagePersonalDatas", new[] { "ItSystemUsage_Id" });
            DropTable("dbo.ItSystemUsagePersonalDatas");
        }
    }
}
