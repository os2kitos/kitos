namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_DataRowUsage : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.LocalFrequencyTypes", newName: "LocalRelationFrequencyTypes");
            DropForeignKey("dbo.DataRowUsage", "DataRowId", "dbo.DataRow");
            DropForeignKey("dbo.FrequencyTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.FrequencyTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataRowUsage", "FrequencyId", "dbo.FrequencyTypes");
            DropForeignKey("dbo.DataRowUsage", new[] { "ItSystemUsageId", "ItSystemId", "ItInterfaceId" }, "dbo.ItInterfaceUsage");
            DropIndex("dbo.DataRowUsage", new[] { "DataRowId" });
            DropIndex("dbo.DataRowUsage", new[] { "ItSystemUsageId", "ItSystemId", "ItInterfaceId" });
            DropIndex("dbo.DataRowUsage", new[] { "FrequencyId" });
            DropIndex("dbo.FrequencyTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.FrequencyTypes", new[] { "LastChangedByUserId" });
            DropTable("dbo.DataRowUsage");
            DropTable("dbo.FrequencyTypes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.FrequencyTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsLocallyAvailable = c.Boolean(nullable: false),
                        IsObligatory = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Description = c.String(),
                        IsEnabled = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DataRowUsage",
                c => new
                    {
                        DataRowId = c.Int(nullable: false),
                        ItSystemUsageId = c.Int(nullable: false),
                        ItSystemId = c.Int(nullable: false),
                        ItInterfaceId = c.Int(nullable: false),
                        FrequencyId = c.Int(),
                        Amount = c.Int(),
                        Economy = c.Int(),
                        Price = c.Int(),
                    })
                .PrimaryKey(t => new { t.DataRowId, t.ItSystemUsageId, t.ItSystemId, t.ItInterfaceId });
            
            CreateIndex("dbo.FrequencyTypes", "LastChangedByUserId");
            CreateIndex("dbo.FrequencyTypes", "ObjectOwnerId");
            CreateIndex("dbo.DataRowUsage", "FrequencyId");
            CreateIndex("dbo.DataRowUsage", new[] { "ItSystemUsageId", "ItSystemId", "ItInterfaceId" });
            CreateIndex("dbo.DataRowUsage", "DataRowId");
            AddForeignKey("dbo.DataRowUsage", new[] { "ItSystemUsageId", "ItSystemId", "ItInterfaceId" }, "dbo.ItInterfaceUsage", new[] { "ItSystemUsageId", "ItSystemId", "ItInterfaceId" }, cascadeDelete: true);
            AddForeignKey("dbo.DataRowUsage", "FrequencyId", "dbo.FrequencyTypes", "Id");
            AddForeignKey("dbo.FrequencyTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.FrequencyTypes", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.DataRowUsage", "DataRowId", "dbo.DataRow", "Id", cascadeDelete: true);
            RenameTable(name: "dbo.LocalRelationFrequencyTypes", newName: "LocalFrequencyTypes");
        }
    }
}
