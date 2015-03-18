namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUniqueUserEmail : DbMigration
    {
        public override void Up()
        {
            CreateIndex("User", "Email", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("User", new[] { "Email" });
        }
    }
}
