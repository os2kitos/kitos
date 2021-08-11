using Infrastructure.DataAccess.Tools;

namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataProcessingRegistration_Added_Uuid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrations", "Uuid", c => c.Guid(nullable: false));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Patch_Uuid_DataProcessingRegistration.sql"));
            CreateIndex("dbo.DataProcessingRegistrations", "Uuid", unique: true, name: "UX_DataProcessingRegistration_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DataProcessingRegistrations", "UX_DataProcessingRegistration_Uuid");
            DropColumn("dbo.DataProcessingRegistrations", "Uuid");
        }
    }
}
