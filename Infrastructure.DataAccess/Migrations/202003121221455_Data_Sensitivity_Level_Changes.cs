using Infrastructure.DataAccess.Tools;

namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Data_Sensitivity_Level_Changes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItSystemUsageSensitiveDataLevels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SensitivityDataLevel = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id, cascadeDelete: true)
                .Index(t => t.ItSystemUsage_Id);

            SqlResource(SqlMigrationScriptRepository.GetResourceName("SensitiveDataLevelUpdate.sql"));

            DropColumn("dbo.ItSystemUsage", "ContainsLegalInfo");
            DropColumn("dbo.ItSystemUsage", "DataLevel");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsage", "DataLevel", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemUsage", "ContainsLegalInfo", c => c.Int(nullable: false));
            DropForeignKey("dbo.ItSystemUsageSensitiveDataLevels", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropIndex("dbo.ItSystemUsageSensitiveDataLevels", new[] { "ItSystemUsage_Id" });
            DropTable("dbo.ItSystemUsageSensitiveDataLevels");
        }
    }
}
