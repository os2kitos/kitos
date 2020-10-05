namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Oversight_Options : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DataProcessingRegistrations", "DataProcessingOversightOption_Id", "dbo.DataProcessingOversightOptions");
            DropIndex("dbo.DataProcessingRegistrations", new[] { "DataProcessingOversightOption_Id" });
            CreateTable(
                "dbo.DataProcessingRegistrationDataProcessingOversightOptions",
                c => new
                    {
                        DataProcessingRegistration_Id = c.Int(nullable: false),
                        DataProcessingOversightOption_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.DataProcessingRegistration_Id, t.DataProcessingOversightOption_Id })
                .ForeignKey("dbo.DataProcessingRegistrations", t => t.DataProcessingRegistration_Id)
                .ForeignKey("dbo.DataProcessingOversightOptions", t => t.DataProcessingOversightOption_Id)
                .Index(t => t.DataProcessingRegistration_Id)
                .Index(t => t.DataProcessingOversightOption_Id);
            
            AddColumn("dbo.DataProcessingRegistrations", "OverSightOptionRemark", c => c.String());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "OversightOptionNamesAsCsv", c => c.String());
            DropColumn("dbo.DataProcessingRegistrations", "DataProcessingOversightOption_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DataProcessingRegistrations", "DataProcessingOversightOption_Id", c => c.Int());
            DropForeignKey("dbo.DataProcessingRegistrationDataProcessingOversightOptions", "DataProcessingOversightOption_Id", "dbo.DataProcessingOversightOptions");
            DropForeignKey("dbo.DataProcessingRegistrationDataProcessingOversightOptions", "DataProcessingRegistration_Id", "dbo.DataProcessingRegistrations");
            DropIndex("dbo.DataProcessingRegistrationDataProcessingOversightOptions", new[] { "DataProcessingOversightOption_Id" });
            DropIndex("dbo.DataProcessingRegistrationDataProcessingOversightOptions", new[] { "DataProcessingRegistration_Id" });
            DropColumn("dbo.DataProcessingRegistrationReadModels", "OversightOptionNamesAsCsv");
            DropColumn("dbo.DataProcessingRegistrations", "OverSightOptionRemark");
            DropTable("dbo.DataProcessingRegistrationDataProcessingOversightOptions");
            CreateIndex("dbo.DataProcessingRegistrations", "DataProcessingOversightOption_Id");
            AddForeignKey("dbo.DataProcessingRegistrations", "DataProcessingOversightOption_Id", "dbo.DataProcessingOversightOptions", "Id");
        }
    }
}
