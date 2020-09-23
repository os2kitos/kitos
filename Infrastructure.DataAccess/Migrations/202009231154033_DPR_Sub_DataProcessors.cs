namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DPR_Sub_DataProcessors : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataProcessingRegistrationOrganization1",
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
            
            AddColumn("dbo.DataProcessingRegistrations", "HasSubDataProcessors", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "SubDataProcessorNamesAsCsv", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProcessingRegistrationOrganization1", "Organization_Id", "dbo.Organization");
            DropForeignKey("dbo.DataProcessingRegistrationOrganization1", "DataProcessingRegistration_Id", "dbo.DataProcessingRegistrations");
            DropIndex("dbo.DataProcessingRegistrationOrganization1", new[] { "Organization_Id" });
            DropIndex("dbo.DataProcessingRegistrationOrganization1", new[] { "DataProcessingRegistration_Id" });
            DropColumn("dbo.DataProcessingRegistrationReadModels", "SubDataProcessorNamesAsCsv");
            DropColumn("dbo.DataProcessingRegistrations", "HasSubDataProcessors");
            DropTable("dbo.DataProcessingRegistrationOrganization1");
        }
    }
}
