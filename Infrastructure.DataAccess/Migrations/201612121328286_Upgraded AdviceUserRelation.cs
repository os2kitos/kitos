namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpgradedAdviceUserRelation : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AdviceUserRelations", "UserId", "dbo.User");
            DropIndex("dbo.AdviceUserRelations", new[] { "UserId" });
            AddColumn("dbo.AdviceUserRelations", "Name", c => c.String());
            AddColumn("dbo.AdviceUserRelations", "RecieverType", c => c.Int(nullable: false));
            DropColumn("dbo.AdviceUserRelations", "RoleId");
            DropColumn("dbo.AdviceUserRelations", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AdviceUserRelations", "UserId", c => c.Int());
            AddColumn("dbo.AdviceUserRelations", "RoleId", c => c.Int());
            DropColumn("dbo.AdviceUserRelations", "RecieverType");
            DropColumn("dbo.AdviceUserRelations", "Name");
            CreateIndex("dbo.AdviceUserRelations", "UserId");
            AddForeignKey("dbo.AdviceUserRelations", "UserId", "dbo.User", "Id");
        }
    }
}
