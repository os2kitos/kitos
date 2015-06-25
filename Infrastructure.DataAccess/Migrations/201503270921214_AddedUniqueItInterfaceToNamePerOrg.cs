namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUniqueItInterfaceToNamePerOrg : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("ItInterface", "FK_ItInterface_Organization_OrganizationId");
            DropIndex("ItInterface", "IX_NamePerOrg");
            Sql("UPDATE ItInterface SET ItInterfaceId='' WHERE ItInterfaceId IS NULL");
            AlterColumn("ItInterface", "ItInterfaceId", c => c.String(nullable: false, maxLength: 100, storeType: "nvarchar"));
            CreateIndex("ItInterface", new[] { "OrganizationId", "Name", "ItInterfaceId" }, unique: true, name: "IX_NamePerOrg");
            AddForeignKey("ItInterface", "OrganizationId", "Organization");
        }
        
        public override void Down()
        {
            throw new NotSupportedException("Down method doesn't include SQL regarding migrating data. If you wish to revert this migration you should implment the SQL needed.");
            DropForeignKey("ItInterface", "FK_ItInterface_Organization_OrganizationId");
            DropIndex("ItInterface", "IX_NamePerOrg");
            AlterColumn("ItInterface", "ItInterfaceId", c => c.String(unicode: false));
            CreateIndex("ItInterface", new[] { "OrganizationId", "Name" }, unique: true, name: "IX_NamePerOrg");
            AddForeignKey("ItInterface", "OrganizationId", "Organization");
        }
    }
}
