namespace Infrastructure.DataAccess.Migrations
{
    using Infrastructure.DataAccess.Tools;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Changed_Running_And_ByEnding_In_Contract_To_Enum : DbMigration
    {
        public override void Up()
        {
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Migrate_Contract_Running_And_ByEnding.sql"));
            AlterColumn("dbo.ItContract", "Running", c => c.Int());
            AlterColumn("dbo.ItContract", "ByEnding", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ItContract", "ByEnding", c => c.String());
            AlterColumn("dbo.ItContract", "Running", c => c.String());
        }
    }
}
