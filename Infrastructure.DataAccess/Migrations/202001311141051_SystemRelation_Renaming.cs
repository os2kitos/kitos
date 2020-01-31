namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SystemRelation_Renaming : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.SystemRelations", name: "RelationSourceId", newName: "FromSystemUsageId");
            RenameColumn(table: "dbo.SystemRelations", name: "RelationTargetId", newName: "ToSystemUsageId");
            RenameIndex(table: "dbo.SystemRelations", name: "IX_RelationSourceId", newName: "IX_FromSystemUsageId");
            RenameIndex(table: "dbo.SystemRelations", name: "IX_RelationTargetId", newName: "IX_ToSystemUsageId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.SystemRelations", name: "IX_ToSystemUsageId", newName: "IX_RelationTargetId");
            RenameIndex(table: "dbo.SystemRelations", name: "IX_FromSystemUsageId", newName: "IX_RelationSourceId");
            RenameColumn(table: "dbo.SystemRelations", name: "ToSystemUsageId", newName: "RelationTargetId");
            RenameColumn(table: "dbo.SystemRelations", name: "FromSystemUsageId", newName: "RelationSourceId");
        }
    }
}
