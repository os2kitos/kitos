namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Uuid_To_DataRow : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataRow", "Uuid", c => c.Guid(nullable: false));
            CreateIndex("dbo.DataRow", "Uuid", unique: true, name: "UX_uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DataRow", "UX_uuid");
            DropColumn("dbo.DataRow", "Uuid");
        }
    }
}
