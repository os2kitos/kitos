namespace Infrastructure.DataAccess.Migrations
{
    using Infrastructure.DataAccess.Tools;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdviceFix : DbMigration
    {
        public override void Up()
        {
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Deactive_Expired_Advice_And_Schedule_Missing_Jobs.sql"));
        }
        
        public override void Down()
        {
        }
    }
}
