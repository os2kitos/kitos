namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUniqueUserEmail : DbMigration
    {
        public override void Up()
        {
            AlterColumn("User", "Email", c => c.String(nullable: false, maxLength: 100, storeType: "nvarchar"));
            CreateIndex("User", "Email", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("User", new[] { "Email" });
            AlterColumn("User", "Email", c => c.String(nullable: false, unicode: false));
        }
    }
}
