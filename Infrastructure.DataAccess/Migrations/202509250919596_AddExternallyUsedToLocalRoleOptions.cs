namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExternallyUsedToLocalRoleOptions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LocalDataProcessingRegistrationRoles", "IsExternallyUsed", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalDataProcessingRegistrationRoles", "ExternallyUsedDescription", c => c.String());
            AddColumn("dbo.LocalItContractRoles", "IsExternallyUsed", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalItContractRoles", "ExternallyUsedDescription", c => c.String());
            AddColumn("dbo.LocalItSystemRoles", "IsExternallyUsed", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalItSystemRoles", "ExternallyUsedDescription", c => c.String());
            AddColumn("dbo.LocalOrganizationUnitRoles", "IsExternallyUsed", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalOrganizationUnitRoles", "ExternallyUsedDescription", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.LocalOrganizationUnitRoles", "ExternallyUsedDescription");
            DropColumn("dbo.LocalOrganizationUnitRoles", "IsExternallyUsed");
            DropColumn("dbo.LocalItSystemRoles", "ExternallyUsedDescription");
            DropColumn("dbo.LocalItSystemRoles", "IsExternallyUsed");
            DropColumn("dbo.LocalItContractRoles", "ExternallyUsedDescription");
            DropColumn("dbo.LocalItContractRoles", "IsExternallyUsed");
            DropColumn("dbo.LocalDataProcessingRegistrationRoles", "ExternallyUsedDescription");
            DropColumn("dbo.LocalDataProcessingRegistrationRoles", "IsExternallyUsed");
        }
    }
}
