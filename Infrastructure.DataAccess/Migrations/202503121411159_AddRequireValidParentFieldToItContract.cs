namespace Infrastructure.DataAccess.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddRequireValidParentFieldToItContract : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContract", "RequireValidParent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItContract", "RequireValidParent");
        }
    }
}
