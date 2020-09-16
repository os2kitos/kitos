namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_DataProcessingAgreementRightsAndRoles : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataProcessingAgreementRights",
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
                .ForeignKey("dbo.DataProcessingAgreements", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.DataProcessingAgreementRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.DataProcessingAgreementRoleAssignmentReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RoleId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        UserFullName = c.String(nullable: false, maxLength: 100),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DataProcessingAgreementReadModels", t => t.ParentId)
                .Index(t => t.RoleId)
                .Index(t => t.UserId)
                .Index(t => t.UserFullName)
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.DataProcessingAgreementRoles",
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
                "dbo.LocalDataProcessingAgreementRoles",
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LocalDataProcessingAgreementRoles", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalDataProcessingAgreementRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalDataProcessingAgreementRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataProcessingAgreementRights", "UserId", "dbo.User");
            DropForeignKey("dbo.DataProcessingAgreementRights", "RoleId", "dbo.DataProcessingAgreementRoles");
            DropForeignKey("dbo.DataProcessingAgreementRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProcessingAgreementRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataProcessingAgreementRights", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProcessingAgreementRights", "ObjectId", "dbo.DataProcessingAgreements");
            DropForeignKey("dbo.DataProcessingAgreementRoleAssignmentReadModels", "ParentId", "dbo.DataProcessingAgreementReadModels");
            DropForeignKey("dbo.DataProcessingAgreementRights", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.LocalDataProcessingAgreementRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalDataProcessingAgreementRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalDataProcessingAgreementRoles", new[] { "OrganizationId" });
            DropIndex("dbo.DataProcessingAgreementRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataProcessingAgreementRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataProcessingAgreementRoleAssignmentReadModels", new[] { "ParentId" });
            DropIndex("dbo.DataProcessingAgreementRoleAssignmentReadModels", new[] { "UserFullName" });
            DropIndex("dbo.DataProcessingAgreementRoleAssignmentReadModels", new[] { "UserId" });
            DropIndex("dbo.DataProcessingAgreementRoleAssignmentReadModels", new[] { "RoleId" });
            DropIndex("dbo.DataProcessingAgreementRights", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataProcessingAgreementRights", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataProcessingAgreementRights", new[] { "ObjectId" });
            DropIndex("dbo.DataProcessingAgreementRights", new[] { "RoleId" });
            DropIndex("dbo.DataProcessingAgreementRights", new[] { "UserId" });
            DropTable("dbo.LocalDataProcessingAgreementRoles");
            DropTable("dbo.DataProcessingAgreementRoles");
            DropTable("dbo.DataProcessingAgreementRoleAssignmentReadModels");
            DropTable("dbo.DataProcessingAgreementRights");
        }
    }
}
