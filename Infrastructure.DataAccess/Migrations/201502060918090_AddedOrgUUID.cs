namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOrgUUID : DbMigration
    {
        public override void Up()
        {
            AddColumn("Organization", "Uuid", c => c.Guid());
        }
        
        public override void Down()
        {
            DropColumn("Organization", "Uuid");
        }
    }
}
