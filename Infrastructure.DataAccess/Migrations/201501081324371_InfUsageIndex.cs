namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InfUsageIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("InfUsage", "ItInterfaceId");
        }
        
        public override void Down()
        {
            DropIndex("InfUsage", new[] { "ItInterfaceId" });
        }
    }
}
