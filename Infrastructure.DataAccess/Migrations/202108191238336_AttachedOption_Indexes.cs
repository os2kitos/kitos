namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AttachedOption_Indexes : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.AttachedOptions", "ObjectId", name: "UX_ObjectId");
            CreateIndex("dbo.AttachedOptions", "ObjectType", name: "UX_ObjectType");
            CreateIndex("dbo.AttachedOptions", "OptionId", name: "UX_OptionId");
            CreateIndex("dbo.AttachedOptions", "OptionType", name: "UX_OptionType");
        }
        
        public override void Down()
        {
            DropIndex("dbo.AttachedOptions", "UX_OptionType");
            DropIndex("dbo.AttachedOptions", "UX_OptionId");
            DropIndex("dbo.AttachedOptions", "UX_ObjectType");
            DropIndex("dbo.AttachedOptions", "UX_ObjectId");
        }
    }
}
