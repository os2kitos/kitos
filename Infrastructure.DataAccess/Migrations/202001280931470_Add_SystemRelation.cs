namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_SystemRelation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SystemRelations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RelationSourceId = c.Int(nullable: false),
                        RelationTargetId = c.Int(nullable: false),
                        Description = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                        AssociatedContract_Id = c.Int(),
                        Reference_Id = c.Int(nullable: false),
                        RelationInterface_Id = c.Int(),
                        UsageFrequency_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContract", t => t.AssociatedContract_Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.ExternalLinks", t => t.Reference_Id, cascadeDelete: true)
                .ForeignKey("dbo.ItInterface", t => t.RelationInterface_Id)
                .ForeignKey("dbo.RelationFrequencyTypes", t => t.UsageFrequency_Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.RelationSourceId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemUsage", t => t.RelationTargetId)
                .Index(t => t.RelationSourceId)
                .Index(t => t.RelationTargetId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.AssociatedContract_Id)
                .Index(t => t.Reference_Id)
                .Index(t => t.RelationInterface_Id)
                .Index(t => t.UsageFrequency_Id);
            
            CreateTable(
                "dbo.ExternalLinks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Url = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.RelationFrequencyTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsLocallyAvailable = c.Boolean(nullable: false),
                        IsObligatory = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Description = c.String(),
                        IsEnabled = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SystemRelations", "RelationTargetId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.SystemRelations", "RelationSourceId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.SystemRelations", "UsageFrequency_Id", "dbo.RelationFrequencyTypes");
            DropForeignKey("dbo.RelationFrequencyTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.RelationFrequencyTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.SystemRelations", "RelationInterface_Id", "dbo.ItInterface");
            DropForeignKey("dbo.SystemRelations", "Reference_Id", "dbo.ExternalLinks");
            DropForeignKey("dbo.ExternalLinks", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ExternalLinks", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.SystemRelations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.SystemRelations", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.SystemRelations", "AssociatedContract_Id", "dbo.ItContract");
            DropIndex("dbo.RelationFrequencyTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.RelationFrequencyTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ExternalLinks", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ExternalLinks", new[] { "ObjectOwnerId" });
            DropIndex("dbo.SystemRelations", new[] { "UsageFrequency_Id" });
            DropIndex("dbo.SystemRelations", new[] { "RelationInterface_Id" });
            DropIndex("dbo.SystemRelations", new[] { "Reference_Id" });
            DropIndex("dbo.SystemRelations", new[] { "AssociatedContract_Id" });
            DropIndex("dbo.SystemRelations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.SystemRelations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.SystemRelations", new[] { "RelationTargetId" });
            DropIndex("dbo.SystemRelations", new[] { "RelationSourceId" });
            DropTable("dbo.RelationFrequencyTypes");
            DropTable("dbo.ExternalLinks");
            DropTable("dbo.SystemRelations");
        }
    }
}
