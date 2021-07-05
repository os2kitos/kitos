namespace Infrastructure.DataAccess.Migrations
{
    using Infrastructure.DataAccess.Tools;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrganizationUnit_Add_UUID : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrganizationUnit", "Uuid", c => c.Guid(nullable: false));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Patch_Uuid_OrganizationUnit.sql"));
            CreateIndex("dbo.OrganizationUnit", "Uuid", unique: true, name: "UX_OrganizationUnit_UUID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.OrganizationUnit", "UX_OrganizationUnit_UUID");
            DropColumn("dbo.OrganizationUnit", "Uuid");
        }
    }
}
