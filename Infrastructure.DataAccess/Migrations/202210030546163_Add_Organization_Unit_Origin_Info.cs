namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Organization_Unit_Origin_Info : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.OrganizationUnit", "UX_LocalId");
            AddColumn("dbo.OrganizationUnit", "Origin", c => c.Int(nullable: false));
            AddColumn("dbo.OrganizationUnit", "ExternalOriginUuid", c => c.Guid());
            CreateIndex("dbo.OrganizationUnit", "ExternalOriginUuid", name: "IX_OrganizationUnit_UUID");
            CreateIndex("dbo.OrganizationUnit", "LocalId", unique: true, name: "UX_LocalId");
            CreateIndex("dbo.OrganizationUnit", "OrganizationId", name: "IX_OrganizationUnit_Origin");
            
            //Initially all organization units are set to 0 = KITOS
            Sql("UPDATE dbo.OrganizationUnit SET Origin = 0;");
        }
        
        public override void Down()
        {
            DropIndex("dbo.OrganizationUnit", "IX_OrganizationUnit_Origin");
            DropIndex("dbo.OrganizationUnit", "UX_LocalId");
            DropIndex("dbo.OrganizationUnit", "IX_OrganizationUnit_UUID");
            DropColumn("dbo.OrganizationUnit", "ExternalOriginUuid");
            DropColumn("dbo.OrganizationUnit", "Origin");
            CreateIndex("dbo.OrganizationUnit", new[] { "OrganizationId", "LocalId" }, unique: true, name: "UX_LocalId");
        }
    }
}
