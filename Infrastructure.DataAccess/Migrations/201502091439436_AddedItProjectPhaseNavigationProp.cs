namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedItProjectPhaseNavigationProp : DbMigration
    {
        public override void Up()
        {
            AddColumn("ItProjectPhases", "ItProjectId", c => c.Int(nullable: false));

            Sql("UPDATE ItprojectPhases a, ItProject b SET a.ItProjectId = b.Id WHERE a.Id = b.Phase1_Id");
            Sql("UPDATE ItprojectPhases a, ItProject b SET a.ItProjectId = b.Id WHERE a.Id = b.Phase2_Id");
            Sql("UPDATE ItprojectPhases a, ItProject b SET a.ItProjectId = b.Id WHERE a.Id = b.Phase3_Id");
            Sql("UPDATE ItprojectPhases a, ItProject b SET a.ItProjectId = b.Id WHERE a.Id = b.Phase4_Id");
            Sql("UPDATE ItprojectPhases a, ItProject b SET a.ItProjectId = b.Id WHERE a.Id = b.Phase5_Id");

            CreateIndex("ItProjectPhases", "ItProjectId");
            AddForeignKey("ItProjectPhases", "ItProjectId", "ItProject", "Id", cascadeDelete: true);

            
        }
        
        public override void Down()
        {
            DropForeignKey("ItProjectPhases", "ItProjectId", "ItProject");
            DropIndex("ItProjectPhases", new[] { "ItProjectId" });
            DropColumn("ItProjectPhases", "ItProjectId");
        }
    }
}
