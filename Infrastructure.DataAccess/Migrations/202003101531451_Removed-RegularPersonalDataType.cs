namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedRegularPersonalDataType : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.LocalRegularPersonalDataTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalRegularPersonalDataTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalRegularPersonalDataTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.RegularPersonalDataTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.RegularPersonalDataTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItSystem", "RegularPersonalDataType_Id", "dbo.RegularPersonalDataTypes");
            DropIndex("dbo.ItSystem", new[] { "RegularPersonalDataType_Id" });
            DropIndex("dbo.LocalRegularPersonalDataTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalRegularPersonalDataTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalRegularPersonalDataTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.RegularPersonalDataTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.RegularPersonalDataTypes", new[] { "LastChangedByUserId" });
            DropColumn("dbo.ItSystem", "RegularPersonalDataType_Id");
            DropTable("dbo.LocalRegularPersonalDataTypes");
            DropTable("dbo.RegularPersonalDataTypes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.RegularPersonalDataTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
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
                "dbo.LocalRegularPersonalDataTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ItSystem", "RegularPersonalDataType_Id", c => c.Int());
            CreateIndex("dbo.RegularPersonalDataTypes", "LastChangedByUserId");
            CreateIndex("dbo.RegularPersonalDataTypes", "ObjectOwnerId");
            CreateIndex("dbo.LocalRegularPersonalDataTypes", "LastChangedByUserId");
            CreateIndex("dbo.LocalRegularPersonalDataTypes", "ObjectOwnerId");
            CreateIndex("dbo.LocalRegularPersonalDataTypes", "OrganizationId");
            CreateIndex("dbo.ItSystem", "RegularPersonalDataType_Id");
            AddForeignKey("dbo.ItSystem", "RegularPersonalDataType_Id", "dbo.RegularPersonalDataTypes", "Id");
            AddForeignKey("dbo.RegularPersonalDataTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.RegularPersonalDataTypes", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.LocalRegularPersonalDataTypes", "OrganizationId", "dbo.Organization", "Id", cascadeDelete: true);
            AddForeignKey("dbo.LocalRegularPersonalDataTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.LocalRegularPersonalDataTypes", "LastChangedByUserId", "dbo.User", "Id");
        }
    }
}
