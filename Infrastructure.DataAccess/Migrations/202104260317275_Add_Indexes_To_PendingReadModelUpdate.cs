namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Indexes_To_PendingReadModelUpdate : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.PendingReadModelUpdates", "SourceId");
            CreateIndex("dbo.PendingReadModelUpdates", "CreatedAt");
            CreateIndex("dbo.PendingReadModelUpdates", "Category");
        }
        
        public override void Down()
        {
            DropIndex("dbo.PendingReadModelUpdates", new[] { "Category" });
            DropIndex("dbo.PendingReadModelUpdates", new[] { "CreatedAt" });
            DropIndex("dbo.PendingReadModelUpdates", new[] { "SourceId" });
        }
    }
}
