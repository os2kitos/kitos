namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Organization_Add_Uuid_UX : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Organization", "Uuid", unique: true, name: "UX_Organization_UUID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Organization", "UX_Organization_UUID");
        }
    }
}
