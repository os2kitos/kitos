namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IconTypeAndIsMainPublicMessage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PublicMessages", "IconType", c => c.Int());
            AddColumn("dbo.PublicMessages", "IsMain", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PublicMessages", "IsMain");
            DropColumn("dbo.PublicMessages", "IconType");
        }
    }
}
