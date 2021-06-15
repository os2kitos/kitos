namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.ItInterface", "Uuid", unique: true, name: "UX_ItInterface_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItInterface", "UX_ItInterface_Uuid");
        }
    }
}
