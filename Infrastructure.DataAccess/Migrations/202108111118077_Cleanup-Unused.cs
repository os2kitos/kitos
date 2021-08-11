namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CleanupUnused : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItSystemUsage", "OverviewId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.TerminationDeadlineTypesInSystems", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.TerminationDeadlineTypesInSystems", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItSystem", "TerminationDeadlineTypesInSystem_Id", "dbo.TerminationDeadlineTypesInSystems");
            DropForeignKey("dbo.ItSystemUsage", "TerminationDeadlineInSystem_Id", "dbo.TerminationDeadlineTypesInSystems");
            DropIndex("dbo.ItSystem", new[] { "TerminationDeadlineTypesInSystem_Id" });
            DropIndex("dbo.ItSystemUsage", new[] { "OverviewId" });
            DropIndex("dbo.ItSystemUsage", new[] { "TerminationDeadlineInSystem_Id" });
            DropIndex("dbo.TerminationDeadlineTypesInSystems", new[] { "ObjectOwnerId" });
            DropIndex("dbo.TerminationDeadlineTypesInSystems", new[] { "LastChangedByUserId" });

            //NOTE: Manually added - otherwise removal will fail since EF does not remove constraint on this field(it appears it was renamed historically and maybe that's why it's broken)
            Sql("alter table dbo.ItSystem drop constraint [FK_dbo.ItSystem_dbo.TerminationDeadlineTypesInSystems_TerminationDeadlineInSystem_Id];");
            DropColumn("dbo.ItSystem", "TerminationDeadlineTypesInSystem_Id");

            DropColumn("dbo.ItSystemUsage", "Terminated");
            DropColumn("dbo.ItSystemUsage", "EsdhRef");
            DropColumn("dbo.ItSystemUsage", "CmdbRef");
            DropColumn("dbo.ItSystemUsage", "OverviewId");
            DropColumn("dbo.ItSystemUsage", "ReportedToDPA");
            DropColumn("dbo.ItSystemUsage", "DocketNo");
            DropColumn("dbo.ItSystemUsage", "ArchivedDate");
            DropColumn("dbo.ItSystemUsage", "TerminationDeadlineInSystem_Id");
            DropTable("dbo.TerminationDeadlineTypesInSystems");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TerminationDeadlineTypesInSystems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        IsLocallyAvailable = c.Boolean(nullable: false),
                        IsObligatory = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Description = c.String(),
                        IsEnabled = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        Uuid = c.Guid(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ItSystemUsage", "TerminationDeadlineInSystem_Id", c => c.Int());
            AddColumn("dbo.ItSystemUsage", "ArchivedDate", c => c.DateTime(storeType: "date"));
            AddColumn("dbo.ItSystemUsage", "DocketNo", c => c.String());
            AddColumn("dbo.ItSystemUsage", "ReportedToDPA", c => c.Boolean());
            AddColumn("dbo.ItSystemUsage", "OverviewId", c => c.Int());
            AddColumn("dbo.ItSystemUsage", "CmdbRef", c => c.String());
            AddColumn("dbo.ItSystemUsage", "EsdhRef", c => c.String());
            AddColumn("dbo.ItSystemUsage", "Terminated", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystem", "TerminationDeadlineTypesInSystem_Id", c => c.Int());
            CreateIndex("dbo.TerminationDeadlineTypesInSystems", "LastChangedByUserId");
            CreateIndex("dbo.TerminationDeadlineTypesInSystems", "ObjectOwnerId");
            CreateIndex("dbo.ItSystemUsage", "TerminationDeadlineInSystem_Id");
            CreateIndex("dbo.ItSystemUsage", "OverviewId");
            CreateIndex("dbo.ItSystem", "TerminationDeadlineTypesInSystem_Id");
            AddForeignKey("dbo.ItSystemUsage", "TerminationDeadlineInSystem_Id", "dbo.TerminationDeadlineTypesInSystems", "Id");
            AddForeignKey("dbo.ItSystem", "TerminationDeadlineTypesInSystem_Id", "dbo.TerminationDeadlineTypesInSystems", "Id");
            AddForeignKey("dbo.TerminationDeadlineTypesInSystems", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.TerminationDeadlineTypesInSystems", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItSystemUsage", "OverviewId", "dbo.ItSystemUsage", "Id");
        }
    }
}
