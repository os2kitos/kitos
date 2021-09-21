namespace Infrastructure.DataAccess.Migrations
{
    using Infrastructure.DataAccess.Tools;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdviceFix : DbMigration
    {
        public override void Up()
        {
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Deactive_Expired_Advice_With_No_Active_HF_Job.sql"));
        }
        
        public override void Down()
        {
        }
    }
}
