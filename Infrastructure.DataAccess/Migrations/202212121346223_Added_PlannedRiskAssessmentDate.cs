namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_PlannedRiskAssessmentDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "PlannedRiskAssessmentDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "PlannedRiskAssessmentDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "PlannedRiskAssessmentDate");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItSystemUsageOverviewReadModels", new[] { "PlannedRiskAssessmentDate" });
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "PlannedRiskAssessmentDate");
            DropColumn("dbo.ItSystemUsage", "PlannedRiskAssessmentDate");
        }
    }
}
