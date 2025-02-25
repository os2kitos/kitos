namespace Infrastructure.DataAccess.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddDBSFieldsToItSystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystem", "DBSName", c => c.String(maxLength: 100));
            AddColumn("dbo.ItSystem", "DBSDataProcessorName", c => c.String(maxLength: 100));
            CreateIndex("dbo.ItSystem", "DBSName", name: "ItSystem_IX_DBSName");
            CreateIndex("dbo.ItSystem", "DBSDataProcessorName", name: "ItSystem_IX_DBSDataProcessorName");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItSystem", "ItSystem_IX_DBSDataProcessorName");
            DropIndex("dbo.ItSystem", "ItSystem_IX_DBSName");
            DropColumn("dbo.ItSystem", "DBSDataProcessorName");
            DropColumn("dbo.ItSystem", "DBSName");
        }
    }
}
