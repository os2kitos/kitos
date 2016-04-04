namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddedLocalIdToOrgUnit : DbMigration
    {
        public override void Up()
        {
            AddColumn("OrganizationUnit", "LocalId", c => c.String(maxLength: 100, storeType: "nvarchar"));
            CreateIndex("OrganizationUnit", new[] { "OrganizationId", "LocalId" }, unique: true, name: "UniqueLocalId");
            DropIndex("OrganizationUnit", new[] { "OrganizationId" });
        }

        public override void Down()
        {
            CreateIndex("OrganizationUnit", "OrganizationId");
            DropIndex("OrganizationUnit", "UniqueLocalId");
            DropColumn("OrganizationUnit", "LocalId");
        }
    }
}
