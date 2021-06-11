namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using Infrastructure.DataAccess.Tools;
    using System.Data.Entity.Migrations;
    
    public partial class Organization_Uuid_Required : DbMigration
    {
        public override void Up()
        {
            SqlResource(SqlMigrationScriptRepository.GetResourceName("AddUuidToOrganizations.sql"));

            AlterColumn("dbo.Organization", "Uuid", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Organization", "Uuid", c => c.Guid());
        }
    }
}
