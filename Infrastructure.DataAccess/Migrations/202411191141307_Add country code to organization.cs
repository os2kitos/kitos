namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addcountrycodetoorganization : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CountryCodes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 150),
                        IsLocallyAvailable = c.Boolean(nullable: false),
                        IsObligatory = c.Boolean(nullable: false),
                        Description = c.String(),
                        IsEnabled = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        Uuid = c.Guid(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.Uuid, unique: true, name: "UX_Option_Uuid")
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            AddColumn("dbo.Organization", "ForeignCountryCodeId", c => c.Int());
            CreateIndex("dbo.Organization", "ForeignCountryCodeId");
            AddForeignKey("dbo.Organization", "ForeignCountryCodeId", "dbo.CountryCodes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Organization", "ForeignCountryCodeId", "dbo.CountryCodes");
            DropForeignKey("dbo.CountryCodes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.CountryCodes", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.CountryCodes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.CountryCodes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.CountryCodes", "UX_Option_Uuid");
            DropIndex("dbo.Organization", new[] { "ForeignCountryCodeId" });
            DropColumn("dbo.Organization", "ForeignCountryCodeId");
            DropTable("dbo.CountryCodes");
        }
    }
}
