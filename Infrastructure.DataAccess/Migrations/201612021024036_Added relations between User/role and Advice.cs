namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedrelationsbetweenUserroleandAdvice : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdviceUserRelations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RoleId = c.Int(),
                        UserId = c.Int(),
                        AviceId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                        Advice_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Advice", t => t.Advice_Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.User", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.Advice_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AdviceUserRelations", "UserId", "dbo.User");
            DropForeignKey("dbo.AdviceUserRelations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.AdviceUserRelations", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.AdviceUserRelations", "Advice_Id", "dbo.Advice");
            DropIndex("dbo.AdviceUserRelations", new[] { "Advice_Id" });
            DropIndex("dbo.AdviceUserRelations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.AdviceUserRelations", new[] { "UserId" });
            DropTable("dbo.AdviceUserRelations");
        }
    }
}
