namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedProcurementPlanHalfToQuarter : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE dbo.ItContract
                  SET ProcurementPlanHalf = 
                        CASE ProcurementPlanHalf
							WHEN 1 THEN 1
							WHEN 2 THEN 3
							ELSE ProcurementPlanHalf
						END;"
            );
            RenameColumn("dbo.ItContract", "ProcurementPlanHalf", "ProcurementPlanQuarter");
        }

        public override void Down()
        {
            Sql(@"UPDATE dbo.ItContract
                  SET ProcurementPlanQuarter = 
                        CASE ProcurementPlanQuarter
							WHEN 1 THEN 1
							WHEN 2 THEN 1
							WHEN 3 THEN 2
							WHEN 4 THEN 2
							ELSE ProcurementPlanQuarter
						END;"
            );
            RenameColumn("dbo.ItContract", "ProcurementPlanQuarter", "ProcurementPlanHalf");
        }
    }
}
