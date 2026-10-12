namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class ChangeMainContractIsActiveToEnum : DbMigration
    {
        public override void Up()
        {
            // First, convert the boolean data to integer values before changing the column type
            Sql(@"
                UPDATE dbo.ItSystemUsageOverviewReadModels 
                SET MainContractIsActive = 
                    CASE 
                        WHEN MainContractId IS NULL THEN 0  -- NoContract
                        WHEN MainContractIsActive = 1 THEN 1  -- Active (was true)
                        ELSE 2  -- Inactive (was false)
                    END;
            ");

            // Now change the column type from bit (bool) to int
            AlterColumn("dbo.ItSystemUsageOverviewReadModels", "MainContractIsActive", c => c.Int(nullable: false));

            // Queue all read models for rebuild to recalculate with the new business logic
            Sql(@"
                INSERT INTO PendingReadModelUpdates (Category, SourceId, CreatedAt)
                SELECT 8, Id, GETUTCDATE()
                FROM dbo.ItSystemUsage
                WHERE NOT EXISTS (
                    SELECT 1 FROM PendingReadModelUpdates 
                    WHERE Category = 8 
                    AND SourceId = dbo.ItSystemUsage.Id
                );
            ");
        }

        public override void Down()
        {
            // Convert enum (int) values back to boolean
            Sql(@"
                UPDATE dbo.ItSystemUsageOverviewReadModels 
                SET MainContractIsActive = 
                    CASE 
                        WHEN MainContractIsActive IN (0, 1) THEN 1  -- NoContract or Active -> true
                        ELSE 0  -- Inactive -> false
                    END;
            ");

            // Change the column type from int back to bit (bool)
            AlterColumn("dbo.ItSystemUsageOverviewReadModels", "MainContractIsActive", c => c.Boolean(nullable: false));
        }
    }
}
