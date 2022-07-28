namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Updated_AdviceUserRelation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AdviceUserRelations", "RoleId", c => c.Int());
            AddColumn("dbo.AdviceUserRelations", "Email", c => c.String());
            DropColumn("dbo.AdviceUserRelations", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AdviceUserRelations", "Name", c => c.String());
            DropColumn("dbo.AdviceUserRelations", "Email");
            DropColumn("dbo.AdviceUserRelations", "RoleId");
        }
    }
}
