namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class FixIndexes : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.OrganizationUnit", "UX_LocalId");
            Sql(@"CREATE UNIQUE NONCLUSTERED INDEX [UX_LocalId]
                  ON [dbo].[OrganizationUnit]([OrganizationId] ASC, [LocalId] ASC)
                  WHERE [LocalId] IS NOT NULL;");

            DropIndex("dbo.Organization", "IX_Cvr");
            Sql(@"CREATE NONCLUSTERED INDEX [IX_Cvr]
                  ON [dbo].[Organization]([Cvr] ASC)
                  WHERE [Cvr] IS NOT NULL;");

            DropIndex("dbo.ItSystem", "UX_NamePerOrg");
            Sql(@"CREATE UNIQUE NONCLUSTERED INDEX [UX_NamePerOrg]
                  ON [dbo].[ItSystem]([OrganizationId] ASC, [Name] ASC)
                  WHERE [Name] IS NOT NULL;");

            DropIndex("dbo.ItInterface", "UX_NamePerOrg");
            Sql(@"CREATE UNIQUE NONCLUSTERED INDEX [UX_NamePerOrg]
                  ON [dbo].[ItInterface]([OrganizationId] ASC, [Name] ASC, [ItInterfaceId] ASC)
                  WHERE [Name] IS NOT NULL;");
        }

        public override void Down()
        {
            DropIndex("dbo.OrganizationUnit", "UX_LocalId");
            CreateIndex("dbo.OrganizationUnit", new[] { "OrganizationId", "LocalId" }, true, "UX_LocalId");

            DropIndex("dbo.Organization", "IX_Cvr");
            CreateIndex("dbo.Organization", "Cvr");

            DropIndex("dbo.ItSystem", "UX_NamePerOrg");
            CreateIndex("dbo.ItSystem", new[] { "OrganizationId", "Name" }, true, "UX_NamePerOrg");

            DropIndex("dbo.ItInterface", "UX_NamePerOrg");
            CreateIndex("dbo.ItInterface", new[] { "OrganizationId", "Name", "ItInterfaceId" }, true, "UX_NamePerOrg");
        }
    }
}
