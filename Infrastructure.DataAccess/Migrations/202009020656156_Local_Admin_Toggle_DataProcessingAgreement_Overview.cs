using Infrastructure.DataAccess.Tools;

namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Local_Admin_Toggle_DataProcessingAgreement_Overview : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Config", "ShowDataProcessingAgreement", c => c.Boolean(nullable: false));

            SqlResource(SqlMigrationScriptRepository.GetResourceName("EnableDataProcessingAgreement.sql"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Config", "ShowDataProcessingAgreement");
        }
    }
}
