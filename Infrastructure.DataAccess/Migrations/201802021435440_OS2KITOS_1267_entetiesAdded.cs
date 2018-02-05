namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OS2KITOS_1267_entetiesAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "systemCategories", c => c.String());
            AddColumn("dbo.ItSystemUsage", "dataProcessor", c => c.String());
            AddColumn("dbo.ItSystemUsage", "dataProcessorControl", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemUsage", "lastControl", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystemUsage", "convictionsData", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemUsage", "precautions", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemUsage", "riskAssessment", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemUsage", "riskAssesmentDate", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystemUsage", "preriskAssessment", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemUsage", "noteRisks", c => c.String());
            AddColumn("dbo.ItSystemUsage", "DPIAhearing", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemUsage", "DPIADate", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystemUsage", "answeringDataDPIA", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemUsage", "DPIAdeleteDate", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystemUsage", "numberDPIA", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsage", "numberDPIA");
            DropColumn("dbo.ItSystemUsage", "DPIAdeleteDate");
            DropColumn("dbo.ItSystemUsage", "answeringDataDPIA");
            DropColumn("dbo.ItSystemUsage", "DPIADate");
            DropColumn("dbo.ItSystemUsage", "DPIAhearing");
            DropColumn("dbo.ItSystemUsage", "noteRisks");
            DropColumn("dbo.ItSystemUsage", "preriskAssessment");
            DropColumn("dbo.ItSystemUsage", "riskAssesmentDate");
            DropColumn("dbo.ItSystemUsage", "riskAssessment");
            DropColumn("dbo.ItSystemUsage", "precautions");
            DropColumn("dbo.ItSystemUsage", "convictionsData");
            DropColumn("dbo.ItSystemUsage", "lastControl");
            DropColumn("dbo.ItSystemUsage", "dataProcessorControl");
            DropColumn("dbo.ItSystemUsage", "dataProcessor");
            DropColumn("dbo.ItSystemUsage", "systemCategories");
        }
    }
}
