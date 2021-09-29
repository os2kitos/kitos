namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_LifeCycleTrackingEvent : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LifeCycleTrackingEvents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventType = c.Int(nullable: false),
                        OccurredAtUtc = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        EntityUuid = c.Guid(nullable: false),
                        EntityType = c.Int(nullable: false),
                        OptionalOrganizationReferenceId = c.Int(),
                        OptionalAccessModifier = c.Int(),
                        OptionalRightsHolderOrganizationId = c.Int(),
                        UserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.OptionalOrganizationReferenceId)
                .ForeignKey("dbo.Organization", t => t.OptionalRightsHolderOrganizationId)
                .ForeignKey("dbo.User", t => t.UserId)
                .Index(t => new { t.EventType, t.OccurredAtUtc, t.EntityType }, name: "IX_EventType_OccurredAt_EntityType_EventType")
                .Index(t => new { t.OptionalOrganizationReferenceId, t.EventType, t.OccurredAtUtc, t.EntityType }, name: "IX_Org_EventType_OccurredAt_EntityType")
                .Index(t => new { t.OptionalRightsHolderOrganizationId, t.OptionalOrganizationReferenceId, t.EventType, t.OccurredAtUtc, t.EntityType }, name: "IX_RightsHolder_Org_EventType_OccurredAt_EntityType")
                .Index(t => new { t.OptionalRightsHolderOrganizationId, t.EventType, t.OccurredAtUtc, t.EntityType }, name: "IX_RightsHolder_EventType_OccurredAt_EntityType")
                .Index(t => new { t.OptionalOrganizationReferenceId, t.OptionalAccessModifier, t.EventType, t.OccurredAtUtc, t.EntityType }, name: "IX_Org_AccessModifier_EventType_OccurredAt_EntityType")
                .Index(t => t.EntityUuid)
                .Index(t => t.OptionalOrganizationReferenceId)
                .Index(t => t.OptionalRightsHolderOrganizationId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LifeCycleTrackingEvents", "UserId", "dbo.User");
            DropForeignKey("dbo.LifeCycleTrackingEvents", "OptionalRightsHolderOrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LifeCycleTrackingEvents", "OptionalOrganizationReferenceId", "dbo.Organization");
            DropIndex("dbo.LifeCycleTrackingEvents", new[] { "UserId" });
            DropIndex("dbo.LifeCycleTrackingEvents", new[] { "OptionalRightsHolderOrganizationId" });
            DropIndex("dbo.LifeCycleTrackingEvents", new[] { "OptionalOrganizationReferenceId" });
            DropIndex("dbo.LifeCycleTrackingEvents", new[] { "EntityUuid" });
            DropIndex("dbo.LifeCycleTrackingEvents", "IX_Org_AccessModifier_EventType_OccurredAt_EntityType");
            DropIndex("dbo.LifeCycleTrackingEvents", "IX_RightsHolder_EventType_OccurredAt_EntityType");
            DropIndex("dbo.LifeCycleTrackingEvents", "IX_RightsHolder_Org_EventType_OccurredAt_EntityType");
            DropIndex("dbo.LifeCycleTrackingEvents", "IX_Org_EventType_OccurredAt_EntityType");
            DropIndex("dbo.LifeCycleTrackingEvents", "IX_EventType_OccurredAt_EntityType_EventType");
            DropTable("dbo.LifeCycleTrackingEvents");
        }
    }
}
