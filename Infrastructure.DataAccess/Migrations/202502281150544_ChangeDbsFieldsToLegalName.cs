namespace Infrastructure.DataAccess.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ChangeDbsFieldsToLegalName : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.ItSystem", "DBSName", "LegalName");
            RenameColumn("dbo.ItSystem", "DBSDataProcessorName", "LegalDataProcessorName");

            RenameIndex("dbo.ItSystem", "ItSystem_IX_DBSName", "ItSystem_IX_LegalName");
            RenameIndex("dbo.ItSystem", "ItSystem_IX_DBSDataProcessorName", "ItSystem_IX_LegalDataProcessorName");
        }

        public override void Down()
        {
            RenameIndex("dbo.ItSystem", "ItSystem_IX_LegalName", "ItSystem_IX_DBSName");
            RenameIndex("dbo.ItSystem", "ItSystem_IX_LegalDataProcessorName", "ItSystem_IX_DBSDataProcessorName");

            RenameColumn("dbo.ItSystem", "LegalName", "DBSName");
            RenameColumn("dbo.ItSystem", "LegalDataProcessorName", "DBSDataProcessorName");
        }
    }
}