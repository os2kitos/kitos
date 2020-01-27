namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_SystemRelation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SystemRelations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                        AssociatedContract_Id = c.Int(),
                        RelationInterface_Id = c.Int(),
                        RelationTarget_Id = c.Int(),
                        UsageFrequency_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContract", t => t.AssociatedContract_Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.ItInterface", t => t.RelationInterface_Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.RelationTarget_Id)
                .ForeignKey("dbo.FrequencyTypes", t => t.UsageFrequency_Id)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.AssociatedContract_Id)
                .Index(t => t.RelationInterface_Id)
                .Index(t => t.RelationTarget_Id)
                .Index(t => t.UsageFrequency_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SystemRelations", "UsageFrequency_Id", "dbo.FrequencyTypes");
            DropForeignKey("dbo.SystemRelations", "RelationTarget_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.SystemRelations", "RelationInterface_Id", "dbo.ItInterface");
            DropForeignKey("dbo.SystemRelations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.SystemRelations", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.SystemRelations", "AssociatedContract_Id", "dbo.ItContract");
            DropIndex("dbo.SystemRelations", new[] { "UsageFrequency_Id" });
            DropIndex("dbo.SystemRelations", new[] { "RelationTarget_Id" });
            DropIndex("dbo.SystemRelations", new[] { "RelationInterface_Id" });
            DropIndex("dbo.SystemRelations", new[] { "AssociatedContract_Id" });
            DropIndex("dbo.SystemRelations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.SystemRelations", new[] { "ObjectOwnerId" });
            DropTable("dbo.SystemRelations");
        }
    }
}
