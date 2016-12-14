namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdvisAddedJobId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Advice", "JobId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Advice", "JobId");
        }
    }
}
