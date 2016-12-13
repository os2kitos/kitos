namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class temp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContract", "DurationYears", c => c.Int());
            AddColumn("dbo.ItContract", "DurationMonths", c => c.Int());
            DropColumn("dbo.ItContract", "Duration");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItContract", "Duration", c => c.Int());
            DropColumn("dbo.ItContract", "DurationMonths");
            DropColumn("dbo.ItContract", "DurationYears");
        }
    }
}
