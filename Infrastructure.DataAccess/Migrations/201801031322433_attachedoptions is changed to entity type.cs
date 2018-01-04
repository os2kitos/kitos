namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class attachedoptionsischangedtoentitytype : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AttachedOptions", "ObjectOwnerId", c => c.Int());
            AddColumn("dbo.AttachedOptions", "LastChanged", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AddColumn("dbo.AttachedOptions", "LastChangedByUserId", c => c.Int());
            CreateIndex("dbo.AttachedOptions", "ObjectOwnerId");
            CreateIndex("dbo.AttachedOptions", "LastChangedByUserId");
            AddForeignKey("dbo.AttachedOptions", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.AttachedOptions", "ObjectOwnerId", "dbo.User", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AttachedOptions", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.AttachedOptions", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.AttachedOptions", new[] { "LastChangedByUserId" });
            DropIndex("dbo.AttachedOptions", new[] { "ObjectOwnerId" });
            DropColumn("dbo.AttachedOptions", "LastChangedByUserId");
            DropColumn("dbo.AttachedOptions", "LastChanged");
            DropColumn("dbo.AttachedOptions", "ObjectOwnerId");
        }
    }
}
