using Infrastructure.DataAccess.Tools;

namespace Infrastructure.DataAccess.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class RemovedEmailBeforeDeletion : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.User", "EmailBeforeDeletion");
            Sql(@"
                UPDATE [User]
                SET Name = 'Slettet bruger',
                    LastName = ''
                WHERE Deleted = 1;"
            );
            
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Migrate_Users_Not_Associated_With_Any_Org.sql"));
        }

        public override void Down()
        {
            AddColumn("dbo.User", "EmailBeforeDeletion", c => c.String());
        }
    }
}
