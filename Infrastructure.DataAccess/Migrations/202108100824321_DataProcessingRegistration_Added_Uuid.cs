namespace Infrastructure.DataAccess.Migrations
{
    using Infrastructure.DataAccess.Tools;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataProcessingRegistration_Added_Uuid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrations", "Uuid", c => c.Guid(nullable: false));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Patch_Uuid_DataProcessingRegistration.sql"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataProcessingRegistrations", "Uuid");
        }
    }
}
