namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _2newoptiontypesadded : DbMigration
    {
        public override void Up()
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.SensitivePersonalDataTypes",
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
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            AddColumn("dbo.ItSystem", "RegularPersonalDataType_Id", c => c.Int());
            AddColumn("dbo.ItSystem", "SensitivePersonalDataType_Id", c => c.Int());
            CreateIndex("dbo.ItSystem", "RegularPersonalDataType_Id");
            CreateIndex("dbo.ItSystem", "SensitivePersonalDataType_Id");
            AddForeignKey("dbo.ItSystem", "RegularPersonalDataType_Id", "dbo.RegularPersonalDataTypes", "Id");
            AddForeignKey("dbo.ItSystem", "SensitivePersonalDataType_Id", "dbo.SensitivePersonalDataTypes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystem", "SensitivePersonalDataType_Id", "dbo.SensitivePersonalDataTypes");
            DropForeignKey("dbo.SensitivePersonalDataTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.SensitivePersonalDataTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystem", "RegularPersonalDataType_Id", "dbo.RegularPersonalDataTypes");
            DropForeignKey("dbo.RegularPersonalDataTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.RegularPersonalDataTypes", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.SensitivePersonalDataTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.SensitivePersonalDataTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.RegularPersonalDataTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.RegularPersonalDataTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystem", new[] { "SensitivePersonalDataType_Id" });
            DropIndex("dbo.ItSystem", new[] { "RegularPersonalDataType_Id" });
            DropColumn("dbo.ItSystem", "SensitivePersonalDataType_Id");
            DropColumn("dbo.ItSystem", "RegularPersonalDataType_Id");
            DropTable("dbo.SensitivePersonalDataTypes");
            DropTable("dbo.RegularPersonalDataTypes");
        }
    }
}
