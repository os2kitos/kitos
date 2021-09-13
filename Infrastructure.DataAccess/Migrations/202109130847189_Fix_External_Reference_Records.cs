using Infrastructure.DataAccess.Tools;
namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Fix_External_Reference_Records : DbMigration
    {
        public override void Up()
        {
            SqlResource(SqlMigrationScriptRepository.GetResourceName("ExternalReferences_Fix_MissingLastUpdated.sql"));
        }

        public override void Down()
        {
        }
    }
}
