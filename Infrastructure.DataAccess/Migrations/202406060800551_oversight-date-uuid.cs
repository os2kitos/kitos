namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class oversightdateuuid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrationOversightDates", "Uuid", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataProcessingRegistrationOversightDates", "Uuid");
        }
    }
}
