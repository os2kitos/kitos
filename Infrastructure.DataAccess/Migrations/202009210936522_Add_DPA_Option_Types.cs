namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_DPA_Option_Types : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataProcessingBasisForTransferOptions",
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.DataProcessingCountryOptions",
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.DataProcessingDataResponsibleOptions",
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.DataProcessingOversightOptions",
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalDataProcessingBasisForTransferOptions",
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalDataProcessingCountryOptions",
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalDataProcessingDataResponsibleOptions",
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalDataProcessingOversightOptions",
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            AddColumn("dbo.DataProcessingAgreements", "DataProcessingBasisForTransferOption_Id", c => c.Int());
            AddColumn("dbo.DataProcessingAgreements", "DataProcessingCountryOption_Id", c => c.Int());
            AddColumn("dbo.DataProcessingAgreements", "DataProcessingDataResponsibleOption_Id", c => c.Int());
            AddColumn("dbo.DataProcessingAgreements", "DataProcessingOversightOption_Id", c => c.Int());
            CreateIndex("dbo.DataProcessingAgreements", "DataProcessingBasisForTransferOption_Id");
            CreateIndex("dbo.DataProcessingAgreements", "DataProcessingCountryOption_Id");
            CreateIndex("dbo.DataProcessingAgreements", "DataProcessingDataResponsibleOption_Id");
            CreateIndex("dbo.DataProcessingAgreements", "DataProcessingOversightOption_Id");
            AddForeignKey("dbo.DataProcessingAgreements", "DataProcessingBasisForTransferOption_Id", "dbo.DataProcessingBasisForTransferOptions", "Id");
            AddForeignKey("dbo.DataProcessingAgreements", "DataProcessingCountryOption_Id", "dbo.DataProcessingCountryOptions", "Id");
            AddForeignKey("dbo.DataProcessingAgreements", "DataProcessingDataResponsibleOption_Id", "dbo.DataProcessingDataResponsibleOptions", "Id");
            AddForeignKey("dbo.DataProcessingAgreements", "DataProcessingOversightOption_Id", "dbo.DataProcessingOversightOptions", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LocalDataProcessingOversightOptions", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalDataProcessingOversightOptions", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalDataProcessingOversightOptions", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalDataProcessingDataResponsibleOptions", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalDataProcessingDataResponsibleOptions", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalDataProcessingDataResponsibleOptions", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalDataProcessingCountryOptions", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalDataProcessingCountryOptions", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalDataProcessingCountryOptions", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalDataProcessingBasisForTransferOptions", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalDataProcessingBasisForTransferOptions", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalDataProcessingBasisForTransferOptions", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataProcessingAgreements", "DataProcessingOversightOption_Id", "dbo.DataProcessingOversightOptions");
            DropForeignKey("dbo.DataProcessingOversightOptions", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProcessingOversightOptions", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataProcessingAgreements", "DataProcessingDataResponsibleOption_Id", "dbo.DataProcessingDataResponsibleOptions");
            DropForeignKey("dbo.DataProcessingDataResponsibleOptions", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProcessingDataResponsibleOptions", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataProcessingAgreements", "DataProcessingCountryOption_Id", "dbo.DataProcessingCountryOptions");
            DropForeignKey("dbo.DataProcessingCountryOptions", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProcessingCountryOptions", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataProcessingAgreements", "DataProcessingBasisForTransferOption_Id", "dbo.DataProcessingBasisForTransferOptions");
            DropForeignKey("dbo.DataProcessingBasisForTransferOptions", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProcessingBasisForTransferOptions", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.LocalDataProcessingOversightOptions", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalDataProcessingOversightOptions", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalDataProcessingOversightOptions", new[] { "OrganizationId" });
            DropIndex("dbo.LocalDataProcessingDataResponsibleOptions", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalDataProcessingDataResponsibleOptions", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalDataProcessingDataResponsibleOptions", new[] { "OrganizationId" });
            DropIndex("dbo.LocalDataProcessingCountryOptions", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalDataProcessingCountryOptions", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalDataProcessingCountryOptions", new[] { "OrganizationId" });
            DropIndex("dbo.LocalDataProcessingBasisForTransferOptions", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalDataProcessingBasisForTransferOptions", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalDataProcessingBasisForTransferOptions", new[] { "OrganizationId" });
            DropIndex("dbo.DataProcessingOversightOptions", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataProcessingOversightOptions", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataProcessingDataResponsibleOptions", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataProcessingDataResponsibleOptions", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataProcessingCountryOptions", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataProcessingCountryOptions", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataProcessingBasisForTransferOptions", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataProcessingBasisForTransferOptions", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataProcessingAgreements", new[] { "DataProcessingOversightOption_Id" });
            DropIndex("dbo.DataProcessingAgreements", new[] { "DataProcessingDataResponsibleOption_Id" });
            DropIndex("dbo.DataProcessingAgreements", new[] { "DataProcessingCountryOption_Id" });
            DropIndex("dbo.DataProcessingAgreements", new[] { "DataProcessingBasisForTransferOption_Id" });
            DropColumn("dbo.DataProcessingAgreements", "DataProcessingOversightOption_Id");
            DropColumn("dbo.DataProcessingAgreements", "DataProcessingDataResponsibleOption_Id");
            DropColumn("dbo.DataProcessingAgreements", "DataProcessingCountryOption_Id");
            DropColumn("dbo.DataProcessingAgreements", "DataProcessingBasisForTransferOption_Id");
            DropTable("dbo.LocalDataProcessingOversightOptions");
            DropTable("dbo.LocalDataProcessingDataResponsibleOptions");
            DropTable("dbo.LocalDataProcessingCountryOptions");
            DropTable("dbo.LocalDataProcessingBasisForTransferOptions");
            DropTable("dbo.DataProcessingOversightOptions");
            DropTable("dbo.DataProcessingDataResponsibleOptions");
            DropTable("dbo.DataProcessingCountryOptions");
            DropTable("dbo.DataProcessingBasisForTransferOptions");
        }
    }
}
