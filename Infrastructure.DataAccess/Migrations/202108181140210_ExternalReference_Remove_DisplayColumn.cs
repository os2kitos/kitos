namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExternalReference_Remove_DisplayColumn : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ExternalReferences", "Display");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ExternalReferences", "Display", c => c.Int(nullable: false));
        }
    }
}
