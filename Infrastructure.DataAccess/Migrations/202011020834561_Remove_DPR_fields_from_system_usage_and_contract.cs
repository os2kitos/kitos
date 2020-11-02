namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Infrastructure.DataAccess.Tools;

    public partial class Remove_DPR_fields_from_system_usage_and_contract : DbMigration
    {
        public override void Up()
        {

            DropIndex("dbo.DataProcessingRegistrations", "DataProcessingRegistration_Index_Name");
            DropIndex("dbo.DataProcessingRegistrationReadModels", "DataProcessingRegistrationReadModel_Index_Name");
            AlterColumn("dbo.DataProcessingRegistrations", "Name", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.DataProcessingRegistrationReadModels", "Name", c => c.String(nullable: false, maxLength: 200));
            CreateIndex("dbo.DataProcessingRegistrations", "Name", name: "DataProcessingRegistration_Index_Name");
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "Name", name: "DataProcessingRegistrationReadModel_Index_Name");

            SqlResource(SqlMigrationScriptRepository.GetResourceName("Migrate_To_Data_Processing_Registration1.sql"));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Migrate_To_Data_Processing_Registration2.sql"));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Migrate_To_Data_Processing_Registration3.sql"));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Migrate_To_Data_Processing_Registration4.sql"));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Migrate_To_Data_Processing_Registration5.sql"));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("AddDPRsToReadModel.sql"));

            DropForeignKey("dbo.ItSystemUsageDataWorkerRelations", "DataWorkerId", "dbo.Organization");
            DropForeignKey("dbo.ItSystemUsageDataWorkerRelations", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsageDataWorkerRelations", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemUsageDataWorkerRelations", "ObjectOwnerId", "dbo.User");
            DropIndex("dbo.ItSystemUsageDataWorkerRelations", new[] { "ItSystemUsageId" });
            DropIndex("dbo.ItSystemUsageDataWorkerRelations", new[] { "DataWorkerId" });
            DropIndex("dbo.ItSystemUsageDataWorkerRelations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemUsageDataWorkerRelations", new[] { "LastChangedByUserId" });
            DropColumn("dbo.ItSystemUsage", "dataProcessor");
            DropColumn("dbo.ItSystemUsage", "dataProcessorControl");
            DropColumn("dbo.ItSystemUsage", "lastControl");
            DropColumn("dbo.ItSystemUsage", "datahandlerSupervisionDocumentationUrlName");
            DropColumn("dbo.ItSystemUsage", "datahandlerSupervisionDocumentationUrl");
            DropColumn("dbo.ItSystemUsage", "noteUsage");
            DropColumn("dbo.ItContract", "ContainsDataHandlerAgreement");
            DropColumn("dbo.ItContract", "DataHandlerAgreementUrlName");
            DropColumn("dbo.ItContract", "DataHandlerAgreementUrl");
            DropTable("dbo.ItSystemUsageDataWorkerRelations");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ItSystemUsageDataWorkerRelations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemUsageId = c.Int(nullable: false),
                        DataWorkerId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ItContract", "DataHandlerAgreementUrl", c => c.String());
            AddColumn("dbo.ItContract", "DataHandlerAgreementUrlName", c => c.String());
            AddColumn("dbo.ItContract", "ContainsDataHandlerAgreement", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemUsage", "noteUsage", c => c.String());
            AddColumn("dbo.ItSystemUsage", "datahandlerSupervisionDocumentationUrl", c => c.String());
            AddColumn("dbo.ItSystemUsage", "datahandlerSupervisionDocumentationUrlName", c => c.String());
            AddColumn("dbo.ItSystemUsage", "lastControl", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystemUsage", "dataProcessorControl", c => c.Int());
            AddColumn("dbo.ItSystemUsage", "dataProcessor", c => c.String());
            DropIndex("dbo.DataProcessingRegistrationReadModels", "DataProcessingRegistrationReadModel_Index_Name");
            DropIndex("dbo.DataProcessingRegistrations", "DataProcessingRegistration_Index_Name");
            AlterColumn("dbo.DataProcessingRegistrationReadModels", "Name", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.DataProcessingRegistrations", "Name", c => c.String(nullable: false, maxLength: 100));
            CreateIndex("dbo.ItSystemUsageDataWorkerRelations", "LastChangedByUserId");
            CreateIndex("dbo.ItSystemUsageDataWorkerRelations", "ObjectOwnerId");
            CreateIndex("dbo.ItSystemUsageDataWorkerRelations", "DataWorkerId");
            CreateIndex("dbo.ItSystemUsageDataWorkerRelations", "ItSystemUsageId");
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "Name", name: "DataProcessingRegistrationReadModel_Index_Name");
            CreateIndex("dbo.DataProcessingRegistrations", "Name", name: "DataProcessingRegistration_Index_Name");
            AddForeignKey("dbo.ItSystemUsageDataWorkerRelations", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.ItSystemUsageDataWorkerRelations", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItSystemUsageDataWorkerRelations", "ItSystemUsageId", "dbo.ItSystemUsage", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItSystemUsageDataWorkerRelations", "DataWorkerId", "dbo.Organization", "Id");
        }
    }
}
