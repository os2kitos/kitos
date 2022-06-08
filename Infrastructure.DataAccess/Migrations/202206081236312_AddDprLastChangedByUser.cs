namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDprLastChangedByUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrationReadModels", "LastChangedById", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "LastChangedByName", c => c.String());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "LastChangedAt", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataProcessingRegistrationReadModels", "LastChangedAt");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "LastChangedByName");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "LastChangedById");
        }
    }
}
