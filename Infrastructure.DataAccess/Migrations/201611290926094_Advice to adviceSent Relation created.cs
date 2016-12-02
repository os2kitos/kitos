namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdvicetoadviceSentRelationcreated : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdviceSents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdviceSentDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        AdviceId = c.Int(),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Advice", t => t.AdviceId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.AdviceId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AdviceSents", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.AdviceSents", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.AdviceSents", "AdviceId", "dbo.Advice");
            DropIndex("dbo.AdviceSents", new[] { "LastChangedByUserId" });
            DropIndex("dbo.AdviceSents", new[] { "ObjectOwnerId" });
            DropIndex("dbo.AdviceSents", new[] { "AdviceId" });
            DropTable("dbo.AdviceSents");
        }
    }
}
