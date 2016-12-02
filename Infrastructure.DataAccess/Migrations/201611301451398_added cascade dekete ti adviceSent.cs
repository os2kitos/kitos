namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedcascadedeketetiadviceSent : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AdviceSents", "AdviceId", "dbo.Advice");
            DropIndex("dbo.AdviceSents", new[] { "AdviceId" });
            AlterColumn("dbo.AdviceSents", "AdviceId", c => c.Int(nullable: false));
            CreateIndex("dbo.AdviceSents", "AdviceId");
            AddForeignKey("dbo.AdviceSents", "AdviceId", "dbo.Advice", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AdviceSents", "AdviceId", "dbo.Advice");
            DropIndex("dbo.AdviceSents", new[] { "AdviceId" });
            AlterColumn("dbo.AdviceSents", "AdviceId", c => c.Int());
            CreateIndex("dbo.AdviceSents", "AdviceId");
            AddForeignKey("dbo.AdviceSents", "AdviceId", "dbo.Advice", "Id");
        }
    }
}
