namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Remove_TaskUsage : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TaskUsage", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.TaskUsage", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.TaskUsage", "OrgUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.TaskUsage", "ParentId", "dbo.TaskUsage");
            DropForeignKey("dbo.TaskUsage", "TaskRefId", "dbo.TaskRef");
            DropIndex("dbo.TaskUsage", new[] { "TaskRefId" });
            DropIndex("dbo.TaskUsage", new[] { "OrgUnitId" });
            DropIndex("dbo.TaskUsage", new[] { "ParentId" });
            DropIndex("dbo.TaskUsage", new[] { "ObjectOwnerId" });
            DropIndex("dbo.TaskUsage", new[] { "LastChangedByUserId" });
            DropColumn("dbo.Config", "ShowTabOverview");
            DropColumn("dbo.Config", "ShowColumnTechnology");
            DropColumn("dbo.Config", "ShowColumnUsage");
            DropTable("dbo.TaskUsage");

            //Fix old preferences
            Sql(@"  UPDATE dbo.[User]
                    SET DefaultUserStartPreference = 'organization.structure' 
                    WHERE DefaultUserStartPreference = 'organization.overview';");
        }

        public override void Down()
        {
            CreateTable(
                "dbo.TaskUsage",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    TaskRefId = c.Int(nullable: false),
                    OrgUnitId = c.Int(nullable: false),
                    ParentId = c.Int(),
                    Starred = c.Boolean(nullable: false),
                    TechnologyStatus = c.Int(nullable: false),
                    UsageStatus = c.Int(nullable: false),
                    Comment = c.String(),
                    ObjectOwnerId = c.Int(nullable: false),
                    LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    LastChangedByUserId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.Config", "ShowColumnUsage", c => c.Boolean(nullable: false));
            AddColumn("dbo.Config", "ShowColumnTechnology", c => c.Boolean(nullable: false));
            AddColumn("dbo.Config", "ShowTabOverview", c => c.Boolean(nullable: false));
            CreateIndex("dbo.TaskUsage", "LastChangedByUserId");
            CreateIndex("dbo.TaskUsage", "ObjectOwnerId");
            CreateIndex("dbo.TaskUsage", "ParentId");
            CreateIndex("dbo.TaskUsage", "OrgUnitId");
            CreateIndex("dbo.TaskUsage", "TaskRefId");
            AddForeignKey("dbo.TaskUsage", "TaskRefId", "dbo.TaskRef", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TaskUsage", "ParentId", "dbo.TaskUsage", "Id");
            AddForeignKey("dbo.TaskUsage", "OrgUnitId", "dbo.OrganizationUnit", "Id");
            AddForeignKey("dbo.TaskUsage", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.TaskUsage", "LastChangedByUserId", "dbo.User", "Id");
        }
    }
}
