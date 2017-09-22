namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ITContractduration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContract", "DurationYears", c => c.Int());
            AddColumn("dbo.ItContract", "DurationMonths", c => c.Int());
            AddColumn("dbo.ItContract", "DurationOngoing", c => c.Boolean(nullable: false));
            Sql("UPDATE [dbo].[ItContract] \r\nSET DurationMonths = Duration\r\nWHERE Duration = 3 OR Duration = 6 OR Duration = 9;\r\n\r\nUPDATE [dbo].[ItContract] \r\nSET DurationYears = 1\r\nWHERE Duration = 12;\r\n\r\nUPDATE [dbo].[ItContract] \r\nSET DurationYears = 1, DurationMonths = 6\r\nWHERE Duration = 18;\r\n\r\nUPDATE [dbo].[ItContract]\r\nSET DurationYears = Duration/12\r\nWHERE Duration >= 24;");
            DropColumn("dbo.ItContract", "Duration");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItContract", "Duration", c => c.Int());
            DropColumn("dbo.ItContract", "DurationOngoing");
            DropColumn("dbo.ItContract", "DurationMonths");
            DropColumn("dbo.ItContract", "DurationYears");
        }
    }
}
