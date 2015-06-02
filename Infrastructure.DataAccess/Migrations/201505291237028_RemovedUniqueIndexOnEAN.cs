namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedUniqueIndexOnEAN : DbMigration
    {
        public override void Up()
        {
            DropIndex("OrganizationUnit", new[] { "Ean" });
        }
        
        public override void Down()
        {
            CreateIndex("OrganizationUnit", "Ean", unique: true);
        }
    }
}
