namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_AccessModifier_From_Economystream : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.EconomyStream", "UX_AccessModifier");
            DropColumn("dbo.EconomyStream", "AccessModifier");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EconomyStream", "AccessModifier", c => c.Int(nullable: false));
            CreateIndex("dbo.EconomyStream", "AccessModifier", name: "UX_AccessModifier");
        }
    }
}
