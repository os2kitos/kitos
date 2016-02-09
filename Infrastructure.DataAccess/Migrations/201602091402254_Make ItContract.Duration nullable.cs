namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class MakeItContractDurationnullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("ItContract", "Duration", c => c.Int());
        }

        public override void Down()
        {
            AlterColumn("ItContract", "Duration", c => c.Int(nullable: false));
        }
    }
}
