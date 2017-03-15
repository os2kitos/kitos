namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DatabaseRelationsshipAddedInItProject : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItProjectStatusUpdates", "AssociatedItProjectId", "dbo.ItProject");
            AddForeignKey("dbo.ItProjectStatusUpdates", "AssociatedItProjectId", "dbo.ItProject", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItProjectStatusUpdates", "AssociatedItProjectId", "dbo.ItProject");
            AddForeignKey("dbo.ItProjectStatusUpdates", "AssociatedItProjectId", "dbo.ItProject", "Id");
        }
    }
}
