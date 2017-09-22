namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeletedStatusProjectfromItProject : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO [dbo].ItProjectStatusUpdates \r\n(AssociatedItProjectId, IsCombined, CombinedStatus, TimeStatus, QualityStatus, ResourcesStatus, Note, ObjectOwnerId, LastChanged, Created, IsFinal, OrganizationId)\r\n(SELECT Id, IsCombined = 1, StatusProject, TimeStatus = 0, QualityStatus = 0, ResourcesStatus = 0, StatusNote, ObjectOwnerId, LastChanged = GETDATE(), Created = LastChanged, IsFinal = 0, OrganizationId FROM [dbo].[ItProject])");
            DropColumn("dbo.ItProject", "StatusProject");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItProject", "StatusProject", c => c.Int(nullable: false));
        }
    }
}
