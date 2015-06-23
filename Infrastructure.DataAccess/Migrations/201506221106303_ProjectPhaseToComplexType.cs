namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectPhaseToComplexType : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("ItProjectPhases", "ItProjectId", "ItProject");
            DropForeignKey("ItProjectPhases", "LastChangedByUserId", "User");
            DropForeignKey("ItProjectPhases", "ObjectOwnerId", "User");
            DropForeignKey("ItProject", "Phase1_Id", "ItProjectPhases");
            DropForeignKey("ItProject", "Phase2_Id", "ItProjectPhases");
            DropForeignKey("ItProject", "Phase3_Id", "ItProjectPhases");
            DropForeignKey("ItProject", "Phase4_Id", "ItProjectPhases");
            DropForeignKey("ItProject", "Phase5_Id", "ItProjectPhases");
            DropIndex("ItProject", new[] { "Phase1_Id" });
            DropIndex("ItProject", new[] { "Phase2_Id" });
            DropIndex("ItProject", new[] { "Phase3_Id" });
            DropIndex("ItProject", new[] { "Phase4_Id" });
            DropIndex("ItProject", new[] { "Phase5_Id" });
            DropIndex("ItProjectPhases", new[] { "ItProjectId" });
            DropIndex("ItProjectPhases", new[] { "ObjectOwnerId" });
            DropIndex("ItProjectPhases", new[] { "LastChangedByUserId" });
            AddColumn("ItProject", "Phase1_Name", c => c.String(unicode: false));
            AddColumn("ItProject", "Phase1_StartDate", c => c.DateTime(precision: 0));
            AddColumn("ItProject", "Phase1_EndDate", c => c.DateTime(precision: 0));
            AddColumn("ItProject", "Phase2_Name", c => c.String(unicode: false));
            AddColumn("ItProject", "Phase2_StartDate", c => c.DateTime(precision: 0));
            AddColumn("ItProject", "Phase2_EndDate", c => c.DateTime(precision: 0));
            AddColumn("ItProject", "Phase3_Name", c => c.String(unicode: false));
            AddColumn("ItProject", "Phase3_StartDate", c => c.DateTime(precision: 0));
            AddColumn("ItProject", "Phase3_EndDate", c => c.DateTime(precision: 0));
            AddColumn("ItProject", "Phase4_Name", c => c.String(unicode: false));
            AddColumn("ItProject", "Phase4_StartDate", c => c.DateTime(precision: 0));
            AddColumn("ItProject", "Phase4_EndDate", c => c.DateTime(precision: 0));
            AddColumn("ItProject", "Phase5_Name", c => c.String(unicode: false));
            AddColumn("ItProject", "Phase5_StartDate", c => c.DateTime(precision: 0));
            AddColumn("ItProject", "Phase5_EndDate", c => c.DateTime(precision: 0));
            AddColumn("ItProject", "CurrentPhase", c => c.Int(nullable: false));

            // move data from phases table into project table
            // as it's stored as a complex type now
            Sql("UPDATE itproject proj " +
                "JOIN itprojectphases phase1 ON proj.Phase1_Id = phase1.Id " +
                "JOIN itprojectphases phase2 ON proj.Phase2_Id = phase2.Id " +
                "JOIN itprojectphases phase3 ON proj.Phase3_Id = phase3.Id " +
                "JOIN itprojectphases phase4 ON proj.Phase4_Id = phase4.Id " +
                "JOIN itprojectphases phase5 ON proj.Phase5_Id = phase5.Id " +
                "SET " +
                "proj.Phase1_Name = phase1.Name, proj.Phase1_StartDate = phase1.StartDate, proj.Phase1_EndDate = phase1.EndDate, " +
                "proj.Phase2_Name = phase2.Name, proj.Phase2_StartDate = phase2.StartDate, proj.Phase2_EndDate = phase2.EndDate, " +
                "proj.Phase3_Name = phase3.Name, proj.Phase3_StartDate = phase3.StartDate, proj.Phase3_EndDate = phase3.EndDate, " +
                "proj.Phase4_Name = phase4.Name, proj.Phase4_StartDate = phase4.StartDate, proj.Phase4_EndDate = phase4.EndDate, " +
                "proj.Phase5_Name = phase5.Name, proj.Phase5_StartDate = phase5.StartDate, proj.Phase5_EndDate = phase5.EndDate, " +
                "proj.CurrentPhase = CASE " +
                    "WHEN proj.CurrentPhaseId = phase1.Id THEN 1 " +
                    "WHEN proj.CurrentPhaseId = phase2.Id THEN 2 " +
                    "WHEN proj.CurrentPhaseId = phase3.Id THEN 3 " +
                    "WHEN proj.CurrentPhaseId = phase4.Id THEN 4 " +
                    "WHEN proj.CurrentPhaseId = phase5.Id THEN 5 " +
                "END");

            DropColumn("ItProject", "CurrentPhaseId");
            DropColumn("ItProject", "Phase1_Id");
            DropColumn("ItProject", "Phase2_Id");
            DropColumn("ItProject", "Phase3_Id");
            DropColumn("ItProject", "Phase4_Id");
            DropColumn("ItProject", "Phase5_Id");
            DropTable("ItProjectPhases");
        }
        
        public override void Down()
        {
            CreateTable(
                "ItProjectPhases",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        StartDate = c.DateTime(precision: 0),
                        EndDate = c.DateTime(precision: 0),
                        ItProjectId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("ItProject", "Phase5_Id", c => c.Int());
            AddColumn("ItProject", "Phase4_Id", c => c.Int());
            AddColumn("ItProject", "Phase3_Id", c => c.Int());
            AddColumn("ItProject", "Phase2_Id", c => c.Int());
            AddColumn("ItProject", "Phase1_Id", c => c.Int());
            AddColumn("ItProject", "CurrentPhaseId", c => c.Int());
            DropColumn("ItProject", "CurrentPhase");
            DropColumn("ItProject", "Phase5_EndDate");
            DropColumn("ItProject", "Phase5_StartDate");
            DropColumn("ItProject", "Phase5_Name");
            DropColumn("ItProject", "Phase4_EndDate");
            DropColumn("ItProject", "Phase4_StartDate");
            DropColumn("ItProject", "Phase4_Name");
            DropColumn("ItProject", "Phase3_EndDate");
            DropColumn("ItProject", "Phase3_StartDate");
            DropColumn("ItProject", "Phase3_Name");
            DropColumn("ItProject", "Phase2_EndDate");
            DropColumn("ItProject", "Phase2_StartDate");
            DropColumn("ItProject", "Phase2_Name");
            DropColumn("ItProject", "Phase1_EndDate");
            DropColumn("ItProject", "Phase1_StartDate");
            DropColumn("ItProject", "Phase1_Name");
            CreateIndex("ItProjectPhases", "LastChangedByUserId");
            CreateIndex("ItProjectPhases", "ObjectOwnerId");
            CreateIndex("ItProjectPhases", "ItProjectId");
            CreateIndex("ItProject", "Phase5_Id");
            CreateIndex("ItProject", "Phase4_Id");
            CreateIndex("ItProject", "Phase3_Id");
            CreateIndex("ItProject", "Phase2_Id");
            CreateIndex("ItProject", "Phase1_Id");
            AddForeignKey("ItProject", "Phase5_Id", "ItProjectPhases", "Id");
            AddForeignKey("ItProject", "Phase4_Id", "ItProjectPhases", "Id");
            AddForeignKey("ItProject", "Phase3_Id", "ItProjectPhases", "Id");
            AddForeignKey("ItProject", "Phase2_Id", "ItProjectPhases", "Id");
            AddForeignKey("ItProject", "Phase1_Id", "ItProjectPhases", "Id");
            AddForeignKey("ItProjectPhases", "ObjectOwnerId", "User", "Id");
            AddForeignKey("ItProjectPhases", "LastChangedByUserId", "User", "Id");
            AddForeignKey("ItProjectPhases", "ItProjectId", "ItProject", "Id", cascadeDelete: true);
        }
    }
}
