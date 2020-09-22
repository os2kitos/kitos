namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Agreement_Oversight_Interval_Option_And_Note : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrations", "OversightInterval", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrations", "OversightIntervalNote", c => c.String());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "OversightInterval", c => c.String());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "OversightIntervalNote", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataProcessingRegistrationReadModels", "OversightIntervalNote");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "OversightInterval");
            DropColumn("dbo.DataProcessingRegistrations", "OversightIntervalNote");
            DropColumn("dbo.DataProcessingRegistrations", "OversightInterval");
        }
    }
}
