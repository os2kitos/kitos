namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedEmailOnDataprocessingRegistration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrationRoleAssignmentReadModels", "Email", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataProcessingRegistrationRoleAssignmentReadModels", "Email");
        }
    }
}
