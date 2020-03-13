namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GDPR_Fields_Nullable_Changes : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ItSystemUsage", "isBusinessCritical", c => c.Int());
            AlterColumn("dbo.ItSystemUsage", "dataProcessorControl", c => c.Int());
            AlterColumn("dbo.ItSystemUsage", "precautions", c => c.Int());
            AlterColumn("dbo.ItSystemUsage", "riskAssessment", c => c.Int());
            AlterColumn("dbo.ItSystemUsage", "preriskAssessment", c => c.Int());
            AlterColumn("dbo.ItSystemUsage", "DPIA", c => c.Int());
            AlterColumn("dbo.ItSystemUsage", "answeringDataDPIA", c => c.Int());
            AlterColumn("dbo.ItSystemUsage", "UserSupervision", c => c.Int());
            DropColumn("dbo.ItSystemUsage", "DPIAhearing");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsage", "DPIAhearing", c => c.Int(nullable: false));
            AlterColumn("dbo.ItSystemUsage", "UserSupervision", c => c.Int(nullable: false));
            AlterColumn("dbo.ItSystemUsage", "answeringDataDPIA", c => c.Int(nullable: false));
            AlterColumn("dbo.ItSystemUsage", "DPIA", c => c.Int(nullable: false));
            AlterColumn("dbo.ItSystemUsage", "preriskAssessment", c => c.Int(nullable: false));
            AlterColumn("dbo.ItSystemUsage", "riskAssessment", c => c.Int(nullable: false));
            AlterColumn("dbo.ItSystemUsage", "precautions", c => c.Int(nullable: false));
            AlterColumn("dbo.ItSystemUsage", "dataProcessorControl", c => c.Int(nullable: false));
            AlterColumn("dbo.ItSystemUsage", "isBusinessCritical", c => c.Int(nullable: false));
        }
    }
}
