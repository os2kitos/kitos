namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedHelpText : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HelpTexts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HelpTexts", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.HelpTexts", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.HelpTexts", new[] { "LastChangedByUserId" });
            DropIndex("dbo.HelpTexts", new[] { "ObjectOwnerId" });
            DropTable("dbo.HelpTexts");
        }
    }
}
