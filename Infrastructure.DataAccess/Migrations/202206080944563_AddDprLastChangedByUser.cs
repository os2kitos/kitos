namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDprLastChangedByUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrationReadModels", "LastChanged", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AddColumn("dbo.DataProcessingRegistrationReadModels", "LastChangedByUserId", c => c.Int(nullable: false));
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "LastChangedByUserId");
            AddForeignKey("dbo.DataProcessingRegistrationReadModels", "LastChangedByUserId", "dbo.User", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProcessingRegistrationReadModels", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.DataProcessingRegistrationReadModels", new[] { "LastChangedByUserId" });
            DropColumn("dbo.DataProcessingRegistrationReadModels", "LastChangedByUserId");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "LastChanged");
        }
    }
}
