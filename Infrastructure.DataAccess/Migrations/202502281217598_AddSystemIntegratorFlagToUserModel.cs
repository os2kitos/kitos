namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSystemIntegratorFlagToUserModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "IsSystemIntegrator", c => c.Boolean(nullable: false));
            CreateIndex("dbo.User", "IsSystemIntegrator", name: "IX_User_IsSystemIntegrator");
        }
        
        public override void Down()
        {
            DropIndex("dbo.User", "IX_User_IsSystemIntegrator");
            DropColumn("dbo.User", "IsSystemIntegrator");
        }
    }
}
