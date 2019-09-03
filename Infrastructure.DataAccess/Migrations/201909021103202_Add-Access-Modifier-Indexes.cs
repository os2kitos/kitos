namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAccessModifierIndexes : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.ItSystem", "AccessModifier", name: "UX_AccessModifier");
            CreateIndex("dbo.Organization", "AccessModifier", name: "UX_AccessModifier");
            CreateIndex("dbo.ItInterface", "AccessModifier", name: "UX_AccessModifier");
            CreateIndex("dbo.ItProject", "AccessModifier", name: "UX_AccessModifier");
            CreateIndex("dbo.EconomyStream", "AccessModifier", name: "UX_AccessModifier");
            CreateIndex("dbo.TaskRef", "AccessModifier", name: "UX_AccessModifier");
            CreateIndex("dbo.Reports", "AccessModifier", name: "UX_AccessModifier");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Reports", "UX_AccessModifier");
            DropIndex("dbo.TaskRef", "UX_AccessModifier");
            DropIndex("dbo.EconomyStream", "UX_AccessModifier");
            DropIndex("dbo.ItProject", "UX_AccessModifier");
            DropIndex("dbo.ItInterface", "UX_AccessModifier");
            DropIndex("dbo.Organization", "UX_AccessModifier");
            DropIndex("dbo.ItSystem", "UX_AccessModifier");
        }
    }
}
