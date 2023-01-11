namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class added_uuid_to_external_reference : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExternalReferences", "Uuid", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExternalReferences", "Uuid");
        }
    }
}
