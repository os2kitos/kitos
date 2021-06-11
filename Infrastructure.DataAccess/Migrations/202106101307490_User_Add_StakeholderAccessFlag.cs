namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class User_Add_StakeholderAccessFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "HasStakeHolderAccess", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "HasStakeHolderAccess");
        }
    }
}
