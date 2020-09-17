namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Extend_DpaReadmodel_With_SystemNames : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingAgreementReadModels", "SystemNamesAsCsv", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataProcessingAgreementReadModels", "SystemNamesAsCsv");
        }
    }
}
