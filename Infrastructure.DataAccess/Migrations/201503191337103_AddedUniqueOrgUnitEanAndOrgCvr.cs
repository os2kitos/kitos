namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUniqueOrgUnitEanAndOrgCvr : DbMigration
    {
        public override void Up()
        {
            CreateIndex("OrganizationUnit", "Ean", unique: true);
            CreateIndex("Organization", "Cvr", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("Organization", new[] { "Cvr" });
            DropIndex("OrganizationUnit", new[] { "Ean" });
        }
    }
}
