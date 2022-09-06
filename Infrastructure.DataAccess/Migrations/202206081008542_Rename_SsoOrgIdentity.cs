namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Rename_SsoOrgIdentity : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.SsoOrganizationIdentities", newName: "StsOrganizationIdentities");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.StsOrganizationIdentities", newName: "SsoOrganizationIdentities");
        }
    }
}
