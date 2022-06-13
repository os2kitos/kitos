namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDprReadModelLastUpdatedBy : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrationReadModels", "LastChangedById", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "LastChangedByName", c => c.String(maxLength: 100));
            AddColumn("dbo.DataProcessingRegistrationReadModels", "LastChangedAt", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "LastChangedById", name: "ItSystemUsageOverviewReadModel_Index_LastChangedById");
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "LastChangedByName", name: "ItSystemUsageOverviewReadModel_Index_LastChangedByName");
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "LastChangedAt", name: "ItSystemUsageOverviewReadModel_Index_LastChangedAt");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DataProcessingRegistrationReadModels", "ItSystemUsageOverviewReadModel_Index_LastChangedAt");
            DropIndex("dbo.DataProcessingRegistrationReadModels", "ItSystemUsageOverviewReadModel_Index_LastChangedByName");
            DropIndex("dbo.DataProcessingRegistrationReadModels", "ItSystemUsageOverviewReadModel_Index_LastChangedById");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "LastChangedAt");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "LastChangedByName");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "LastChangedById");
        }
    }
}
