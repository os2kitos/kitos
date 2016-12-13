namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class miscchangestoadvis : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.AdviceUserRelations", name: "Advice_Id", newName: "AdviceId");
            RenameIndex(table: "dbo.AdviceUserRelations", name: "IX_Advice_Id", newName: "IX_AdviceId");
            AddColumn("dbo.AdviceUserRelations", "RecpientType", c => c.Int(nullable: false));
            DropColumn("dbo.AdviceUserRelations", "AviceId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AdviceUserRelations", "AviceId", c => c.Int(nullable: false));
            DropColumn("dbo.AdviceUserRelations", "RecpientType");
            RenameIndex(table: "dbo.AdviceUserRelations", name: "IX_AdviceId", newName: "IX_Advice_Id");
            RenameColumn(table: "dbo.AdviceUserRelations", name: "AdviceId", newName: "Advice_Id");
        }
    }
}
