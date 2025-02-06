namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Parent_Usage_Uuid_On_Read_Model : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "ParentItSystemUsageUuid", c => c.Guid());
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "ParentItSystemUsageUuid", name: "ItSystemUsageOverviewReadModel_Index_ParentItSystemUsageUuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ParentItSystemUsageUuid");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "ParentItSystemUsageUuid");
        }
    }
}
