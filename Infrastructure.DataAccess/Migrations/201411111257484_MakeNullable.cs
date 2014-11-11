namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeNullable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("ItProject", "ItProjectTypeId", "ItProjectTypes");
            DropForeignKey("Goal", "GoalTypeId", "GoalTypes");
            DropIndex("ItProject", new[] { "ItProjectTypeId" });
            DropIndex("Goal", new[] { "GoalTypeId" });
            AlterColumn("ItProject", "ItProjectTypeId", c => c.Int());
            AlterColumn("Goal", "GoalTypeId", c => c.Int());
            CreateIndex("ItProject", "ItProjectTypeId");
            CreateIndex("Goal", "GoalTypeId");
            AddForeignKey("ItProject", "ItProjectTypeId", "ItProjectTypes", "Id");
            AddForeignKey("Goal", "GoalTypeId", "GoalTypes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("Goal", "GoalTypeId", "GoalTypes");
            DropForeignKey("ItProject", "ItProjectTypeId", "ItProjectTypes");
            DropIndex("Goal", new[] { "GoalTypeId" });
            DropIndex("ItProject", new[] { "ItProjectTypeId" });
            AlterColumn("Goal", "GoalTypeId", c => c.Int(nullable: false));
            AlterColumn("ItProject", "ItProjectTypeId", c => c.Int(nullable: false));
            CreateIndex("Goal", "GoalTypeId");
            CreateIndex("ItProject", "ItProjectTypeId");
            AddForeignKey("Goal", "GoalTypeId", "GoalTypes", "Id", cascadeDelete: true);
            AddForeignKey("ItProject", "ItProjectTypeId", "ItProjectTypes", "Id", cascadeDelete: true);
        }
    }
}
