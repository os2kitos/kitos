using Infrastructure.DataAccess.Tools;

namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataProcessingRegistration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataProcessingRegistrationRights",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        ObjectId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.DataProcessingRegistrations", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.DataProcessingRegistrationRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.DataProcessingRegistrations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        OrganizationId = c.Int(nullable: false),
                        ReferenceId = c.Int(),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                        DataProcessingBasisForTransferOption_Id = c.Int(),
                        DataProcessingCountryOption_Id = c.Int(),
                        DataProcessingDataResponsibleOption_Id = c.Int(),
                        DataProcessingOversightOption_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .ForeignKey("dbo.ExternalReferences", t => t.ReferenceId)
                .ForeignKey("dbo.DataProcessingBasisForTransferOptions", t => t.DataProcessingBasisForTransferOption_Id)
                .ForeignKey("dbo.DataProcessingCountryOptions", t => t.DataProcessingCountryOption_Id)
                .ForeignKey("dbo.DataProcessingDataResponsibleOptions", t => t.DataProcessingDataResponsibleOption_Id)
                .ForeignKey("dbo.DataProcessingOversightOptions", t => t.DataProcessingOversightOption_Id)
                .Index(t => t.Name, name: "DataProcessingRegistration_Index_Name")
                .Index(t => t.OrganizationId)
                .Index(t => t.ReferenceId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.DataProcessingBasisForTransferOption_Id)
                .Index(t => t.DataProcessingCountryOption_Id)
                .Index(t => t.DataProcessingDataResponsibleOption_Id)
                .Index(t => t.DataProcessingOversightOption_Id);
            
            CreateTable(
                "dbo.DataProcessingRegistrationReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        OrganizationId = c.Int(nullable: false),
                        SourceEntityId = c.Int(nullable: false),
                        MainReferenceUserAssignedId = c.String(),
                        MainReferenceUrl = c.String(),
                        MainReferenceTitle = c.String(maxLength: 100),
                        SystemNamesAsCsv = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .ForeignKey("dbo.DataProcessingRegistrations", t => t.SourceEntityId)
                .Index(t => t.Name, name: "DataProcessingRegistrationReadModel_Index_Name")
                .Index(t => t.OrganizationId)
                .Index(t => t.SourceEntityId)
                .Index(t => t.MainReferenceTitle, name: "DataProcessingRegistrationReadModel_Index_MainReferenceTitle");
            
            CreateTable(
                "dbo.DataProcessingRegistrationRoleAssignmentReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RoleId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        UserFullName = c.String(nullable: false, maxLength: 100),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DataProcessingRegistrationReadModels", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.RoleId)
                .Index(t => t.UserId)
                .Index(t => t.UserFullName)
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.DataProcessingRegistrationRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
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
            
            CreateTable(
                "dbo.LocalDataProcessingRegistrationRoles",
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
                "dbo.PendingReadModelUpdates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SourceId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Category = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DataProcessingRegistrationItSystemUsages",
                c => new
                    {
                        DataProcessingRegistration_Id = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.DataProcessingRegistration_Id, t.ItSystemUsage_Id })
                .ForeignKey("dbo.DataProcessingRegistrations", t => t.DataProcessingRegistration_Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .Index(t => t.DataProcessingRegistration_Id)
                .Index(t => t.ItSystemUsage_Id);
            
            AddColumn("dbo.ExternalReferences", "DataProcessingRegistration_Id", c => c.Int());
            CreateIndex("dbo.ExternalReferences", "DataProcessingRegistration_Id");
            AddForeignKey("dbo.ExternalReferences", "DataProcessingRegistration_Id", "dbo.DataProcessingRegistrations", "Id", cascadeDelete: true);
            SqlResource(SqlMigrationScriptRepository.GetResourceName("AddDefaultDpaTypesToExistingDb.sql"));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LocalDataProcessingRegistrationRoles", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalDataProcessingRegistrationRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalDataProcessingRegistrationRoles", "LastChangedByUserId", "dbo.User");
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
            DropForeignKey("dbo.DataProcessingRegistrations", "DataProcessingOversightOption_Id", "dbo.DataProcessingOversightOptions");
            DropForeignKey("dbo.DataProcessingOversightOptions", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProcessingOversightOptions", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataProcessingRegistrations", "DataProcessingDataResponsibleOption_Id", "dbo.DataProcessingDataResponsibleOptions");
            DropForeignKey("dbo.DataProcessingDataResponsibleOptions", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProcessingDataResponsibleOptions", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataProcessingRegistrations", "DataProcessingCountryOption_Id", "dbo.DataProcessingCountryOptions");
            DropForeignKey("dbo.DataProcessingCountryOptions", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProcessingCountryOptions", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataProcessingRegistrations", "DataProcessingBasisForTransferOption_Id", "dbo.DataProcessingBasisForTransferOptions");
            DropForeignKey("dbo.DataProcessingBasisForTransferOptions", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProcessingBasisForTransferOptions", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataProcessingRegistrationRights", "UserId", "dbo.User");
            DropForeignKey("dbo.DataProcessingRegistrationRights", "RoleId", "dbo.DataProcessingRegistrationRoles");
            DropForeignKey("dbo.DataProcessingRegistrationRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProcessingRegistrationRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataProcessingRegistrationRights", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProcessingRegistrationRights", "ObjectId", "dbo.DataProcessingRegistrations");
            DropForeignKey("dbo.DataProcessingRegistrationItSystemUsages", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.DataProcessingRegistrationItSystemUsages", "DataProcessingRegistration_Id", "dbo.DataProcessingRegistrations");
            DropForeignKey("dbo.DataProcessingRegistrations", "ReferenceId", "dbo.ExternalReferences");
            DropForeignKey("dbo.DataProcessingRegistrations", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.DataProcessingRegistrations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProcessingRegistrations", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ExternalReferences", "DataProcessingRegistration_Id", "dbo.DataProcessingRegistrations");
            DropForeignKey("dbo.DataProcessingRegistrationReadModels", "SourceEntityId", "dbo.DataProcessingRegistrations");
            DropForeignKey("dbo.DataProcessingRegistrationRoleAssignmentReadModels", "ParentId", "dbo.DataProcessingRegistrationReadModels");
            DropForeignKey("dbo.DataProcessingRegistrationReadModels", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.DataProcessingRegistrationRights", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.DataProcessingRegistrationItSystemUsages", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.DataProcessingRegistrationItSystemUsages", new[] { "DataProcessingRegistration_Id" });
            DropIndex("dbo.LocalDataProcessingRegistrationRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalDataProcessingRegistrationRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalDataProcessingRegistrationRoles", new[] { "OrganizationId" });
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
            DropIndex("dbo.DataProcessingRegistrationRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataProcessingRegistrationRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataProcessingRegistrationRoleAssignmentReadModels", new[] { "ParentId" });
            DropIndex("dbo.DataProcessingRegistrationRoleAssignmentReadModels", new[] { "UserFullName" });
            DropIndex("dbo.DataProcessingRegistrationRoleAssignmentReadModels", new[] { "UserId" });
            DropIndex("dbo.DataProcessingRegistrationRoleAssignmentReadModels", new[] { "RoleId" });
            DropIndex("dbo.DataProcessingRegistrationReadModels", "DataProcessingRegistrationReadModel_Index_MainReferenceTitle");
            DropIndex("dbo.DataProcessingRegistrationReadModels", new[] { "SourceEntityId" });
            DropIndex("dbo.DataProcessingRegistrationReadModels", new[] { "OrganizationId" });
            DropIndex("dbo.DataProcessingRegistrationReadModels", "DataProcessingRegistrationReadModel_Index_Name");
            DropIndex("dbo.ExternalReferences", new[] { "DataProcessingRegistration_Id" });
            DropIndex("dbo.DataProcessingRegistrations", new[] { "DataProcessingOversightOption_Id" });
            DropIndex("dbo.DataProcessingRegistrations", new[] { "DataProcessingDataResponsibleOption_Id" });
            DropIndex("dbo.DataProcessingRegistrations", new[] { "DataProcessingCountryOption_Id" });
            DropIndex("dbo.DataProcessingRegistrations", new[] { "DataProcessingBasisForTransferOption_Id" });
            DropIndex("dbo.DataProcessingRegistrations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataProcessingRegistrations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataProcessingRegistrations", new[] { "ReferenceId" });
            DropIndex("dbo.DataProcessingRegistrations", new[] { "OrganizationId" });
            DropIndex("dbo.DataProcessingRegistrations", "DataProcessingRegistration_Index_Name");
            DropIndex("dbo.DataProcessingRegistrationRights", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataProcessingRegistrationRights", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataProcessingRegistrationRights", new[] { "ObjectId" });
            DropIndex("dbo.DataProcessingRegistrationRights", new[] { "RoleId" });
            DropIndex("dbo.DataProcessingRegistrationRights", new[] { "UserId" });
            DropColumn("dbo.ExternalReferences", "DataProcessingRegistration_Id");
            DropTable("dbo.DataProcessingRegistrationItSystemUsages");
            DropTable("dbo.PendingReadModelUpdates");
            DropTable("dbo.LocalDataProcessingRegistrationRoles");
            DropTable("dbo.LocalDataProcessingOversightOptions");
            DropTable("dbo.LocalDataProcessingDataResponsibleOptions");
            DropTable("dbo.LocalDataProcessingCountryOptions");
            DropTable("dbo.LocalDataProcessingBasisForTransferOptions");
            DropTable("dbo.DataProcessingOversightOptions");
            DropTable("dbo.DataProcessingDataResponsibleOptions");
            DropTable("dbo.DataProcessingCountryOptions");
            DropTable("dbo.DataProcessingBasisForTransferOptions");
            DropTable("dbo.DataProcessingRegistrationRoles");
            DropTable("dbo.DataProcessingRegistrationRoleAssignmentReadModels");
            DropTable("dbo.DataProcessingRegistrationReadModels");
            DropTable("dbo.DataProcessingRegistrations");
            DropTable("dbo.DataProcessingRegistrationRights");
        }
    }
}
