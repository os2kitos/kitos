namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedUUID : DbMigration
    {
        public override void Up()
        {
            AddColumn("[dbo].[User]", "UniqueId", c => c.String());
            Sql("UPDATE \"dbo\".\"User\" SET UniqueId = Uuid");
            DropColumn("[dbo].[User]", "Uuid");
        }
        
        public override void Down()
        {                                     
            AddColumn("[dbo].[User]", "Uuid", c => c.Guid());
            Sql("UPDATE \"dbo\".\"User\" SET Uuid = UniqueId");
            DropColumn("[dbo].[User]", "UniqueId");
        }
    }
}
