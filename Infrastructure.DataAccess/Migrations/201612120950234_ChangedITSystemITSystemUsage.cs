namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedITSystemITSystemUsage : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.ItSystem", name: "TerminationDeadlineInSystem_Id", newName: "TerminationDeadlineTypesInSystem_Id");
            RenameIndex(table: "dbo.ItSystem", name: "IX_TerminationDeadlineInSystem_Id", newName: "IX_TerminationDeadlineTypesInSystem_Id");
            AddColumn("dbo.ItSystemUsage", "Active", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemUsage", "Concluded", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystemUsage", "ExpirationDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystemUsage", "Terminated", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystemUsage", "TerminationDeadlineInSystem_Id", c => c.Int());
            CreateIndex("dbo.ItSystemUsage", "TerminationDeadlineInSystem_Id");
            AddForeignKey("dbo.ItSystemUsage", "TerminationDeadlineInSystem_Id", "dbo.TerminationDeadlineTypesInSystems", "Id");
            DropColumn("dbo.ItSystem", "Active");
            DropColumn("dbo.ItSystem", "Concluded");
            DropColumn("dbo.ItSystem", "ExpirationDate");
            DropColumn("dbo.ItSystem", "Terminated");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystem", "Terminated", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystem", "ExpirationDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystem", "Concluded", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystem", "Active", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.ItSystemUsage", "TerminationDeadlineInSystem_Id", "dbo.TerminationDeadlineTypesInSystems");
            DropIndex("dbo.ItSystemUsage", new[] { "TerminationDeadlineInSystem_Id" });
            DropColumn("dbo.ItSystemUsage", "TerminationDeadlineInSystem_Id");
            DropColumn("dbo.ItSystemUsage", "Terminated");
            DropColumn("dbo.ItSystemUsage", "ExpirationDate");
            DropColumn("dbo.ItSystemUsage", "Concluded");
            DropColumn("dbo.ItSystemUsage", "Active");
            RenameIndex(table: "dbo.ItSystem", name: "IX_TerminationDeadlineTypesInSystem_Id", newName: "IX_TerminationDeadlineInSystem_Id");
            RenameColumn(table: "dbo.ItSystem", name: "TerminationDeadlineTypesInSystem_Id", newName: "TerminationDeadlineInSystem_Id");
        }
    }
}
