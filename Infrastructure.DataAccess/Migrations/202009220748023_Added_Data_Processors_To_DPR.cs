namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Data_Processors_To_DPR : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataProcessingRegistrationOrganizations",
                c => new
                    {
                        DataProcessingRegistration_Id = c.Int(nullable: false),
                        Organization_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.DataProcessingRegistration_Id, t.Organization_Id })
                .ForeignKey("dbo.DataProcessingRegistrations", t => t.DataProcessingRegistration_Id)
                .ForeignKey("dbo.Organization", t => t.Organization_Id)
                .Index(t => t.DataProcessingRegistration_Id)
                .Index(t => t.Organization_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProcessingRegistrationOrganizations", "Organization_Id", "dbo.Organization");
            DropForeignKey("dbo.DataProcessingRegistrationOrganizations", "DataProcessingRegistration_Id", "dbo.DataProcessingRegistrations");
            DropIndex("dbo.DataProcessingRegistrationOrganizations", new[] { "Organization_Id" });
            DropIndex("dbo.DataProcessingRegistrationOrganizations", new[] { "DataProcessingRegistration_Id" });
            DropTable("dbo.DataProcessingRegistrationOrganizations");
        }
    }
}
