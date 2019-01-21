namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class accessmodifierremovedfromitcontractremark : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ItContractRemarks", "AccessModifier");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItContractRemarks", "AccessModifier", c => c.Int(nullable: false));
        }
    }
}
