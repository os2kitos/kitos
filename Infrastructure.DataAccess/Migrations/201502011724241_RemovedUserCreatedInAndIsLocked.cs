namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedUserCreatedInAndIsLocked : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("User", "CreatedInId", "Organization");
            DropIndex("User", new[] { "CreatedInId" });
            //DropIndex("InfUsage", new[] { "ItInterfaceId" });
            DropColumn("User", "IsLocked");
            DropColumn("User", "CreatedInId");
        }
        
        public override void Down()
        {
            AddColumn("User", "CreatedInId", c => c.Int());
            AddColumn("User", "IsLocked", c => c.Boolean(nullable: false));
            //CreateIndex("InfUsage", "ItInterfaceId");
            CreateIndex("User", "CreatedInId");
            AddForeignKey("User", "CreatedInId", "Organization", "Id");
        }
    }
}
