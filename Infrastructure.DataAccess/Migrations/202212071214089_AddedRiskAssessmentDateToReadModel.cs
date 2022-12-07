namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedRiskAssessmentDateToReadModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "RiskAssessmentDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "RiskAssessmentDate");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItSystemUsageOverviewReadModels", new[] { "RiskAssessmentDate" });
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "RiskAssessmentDate");
        }
    }
}
