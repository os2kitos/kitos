using Infrastructure.DataAccess.Tools;

namespace Infrastructure.DataAccess.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Remove_ReadOnly_Roles : DbMigration
    {
        public override void Up()
        {
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Remove_ReadOnly_Role.sql"));
        }
        
        public override void Down()
        {
        }
    }
}
