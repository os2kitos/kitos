namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cascade : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AdviceUserRelations", "AdviceId", "dbo.Advice");
            DropIndex("dbo.AdviceUserRelations", new[] { "AdviceId" });
            AlterColumn("dbo.AdviceUserRelations", "AdviceId", c => c.Int(nullable: false));
            CreateIndex("dbo.AdviceUserRelations", "AdviceId");
            AddForeignKey("dbo.AdviceUserRelations", "AdviceId", "dbo.Advice", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AdviceUserRelations", "AdviceId", "dbo.Advice");
            DropIndex("dbo.AdviceUserRelations", new[] { "AdviceId" });
            AlterColumn("dbo.AdviceUserRelations", "AdviceId", c => c.Int());
            CreateIndex("dbo.AdviceUserRelations", "AdviceId");
            AddForeignKey("dbo.AdviceUserRelations", "AdviceId", "dbo.Advice", "Id");
        }
    }
}
