namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_LocallyUniqueNameConstraints : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.DataProcessingRegistrations", new[] { "OrganizationId" });
            DropIndex("dbo.ItContract", new[] { "OrganizationId" });
            DropIndex("dbo.ItProject", new[] { "OrganizationId" });
            CreateIndex("dbo.ItSystem", new[] { "OrganizationId", "Name" }, unique: true, name: "UX_OrganizationLocallyUniqueName");
            CreateIndex("dbo.DataProcessingRegistrations", new[] { "OrganizationId", "Name" }, unique: true, name: "UX_OrganizationLocallyUniqueName");
            CreateIndex("dbo.ItContract", new[] { "OrganizationId", "Name" }, unique: true, name: "UX_OrganizationLocallyUniqueName");
            CreateIndex("dbo.ItProject", new[] { "OrganizationId", "Name" }, unique: true, name: "UX_OrganizationLocallyUniqueName");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItProject", "UX_OrganizationLocallyUniqueName");
            DropIndex("dbo.ItContract", "UX_OrganizationLocallyUniqueName");
            DropIndex("dbo.DataProcessingRegistrations", "UX_OrganizationLocallyUniqueName");
            DropIndex("dbo.ItSystem", "UX_OrganizationLocallyUniqueName");
            CreateIndex("dbo.ItProject", "OrganizationId");
            CreateIndex("dbo.ItContract", "OrganizationId");
            CreateIndex("dbo.DataProcessingRegistrations", "OrganizationId");
        }
    }
}
