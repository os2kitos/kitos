namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AlterOrganizationRole : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.OrganizationRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.OrganizationRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.OrganizationRights", "RoleId", "dbo.OrganizationRoles");
            DropIndex("dbo.OrganizationRights", new[] { "RoleId" });
            DropIndex("dbo.OrganizationRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.OrganizationRoles", new[] { "LastChangedByUserId" });
            RenameColumn(table: "dbo.OrganizationRights", name: "ObjectId", newName: "OrganizationId");
            RenameIndex(table: "dbo.OrganizationRights", name: "IX_ObjectId", newName: "IX_OrganizationId");
            AddColumn("dbo.OrganizationRights", "Role", c => c.Int(nullable: false));

            // migrate local admins
            Sql("UPDATE dbo.OrganizationRights SET Role=5 WHERE RoleId=1");
            // migrate "Medarbejder" users roles
            Sql("UPDATE dbo.OrganizationRights SET Role=0 WHERE RoleId=2");

            DropColumn("dbo.OrganizationRights", "RoleId");
            DropTable("dbo.OrganizationRoles");
        }

        public override void Down()
        {
            CreateTable(
                "dbo.OrganizationRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.OrganizationRights", "RoleId", c => c.Int(nullable: false));
            DropColumn("dbo.OrganizationRights", "Role");
            RenameIndex(table: "dbo.OrganizationRights", name: "IX_OrganizationId", newName: "IX_ObjectId");
            RenameColumn(table: "dbo.OrganizationRights", name: "OrganizationId", newName: "ObjectId");
            CreateIndex("dbo.OrganizationRoles", "LastChangedByUserId");
            CreateIndex("dbo.OrganizationRoles", "ObjectOwnerId");
            CreateIndex("dbo.OrganizationRights", "RoleId");
            AddForeignKey("dbo.OrganizationRights", "RoleId", "dbo.OrganizationRoles", "Id", cascadeDelete: true);
            AddForeignKey("dbo.OrganizationRoles", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.OrganizationRoles", "LastChangedByUserId", "dbo.User", "Id");
        }
    }
}
