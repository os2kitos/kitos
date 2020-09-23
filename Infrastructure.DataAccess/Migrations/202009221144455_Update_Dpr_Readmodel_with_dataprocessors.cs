namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_Dpr_Readmodel_with_dataprocessors : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrationReadModels", "DataProcessorNamesAsCsv", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataProcessingRegistrationReadModels", "DataProcessorNamesAsCsv");
        }
    }
}
