namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedActiveToITSystem : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TerminationDeadlineTypesInSystems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        IsLocallyAvailable = c.Boolean(nullable: false),
                        IsObligatory = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Description = c.String(),
                        IsEnabled = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            AddColumn("dbo.ItSystem", "Active", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystem", "Concluded", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystem", "ExpirationDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystem", "Terminated", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystem", "TerminationDeadlineInSystem_Id", c => c.Int());
            CreateIndex("dbo.ItSystem", "TerminationDeadlineInSystem_Id");
            AddForeignKey("dbo.ItSystem", "TerminationDeadlineInSystem_Id", "dbo.TerminationDeadlineTypesInSystems", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystem", "TerminationDeadlineInSystem_Id", "dbo.TerminationDeadlineTypesInSystems");
            DropForeignKey("dbo.TerminationDeadlineTypesInSystems", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.TerminationDeadlineTypesInSystems", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.TerminationDeadlineTypesInSystems", new[] { "LastChangedByUserId" });
            DropIndex("dbo.TerminationDeadlineTypesInSystems", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystem", new[] { "TerminationDeadlineInSystem_Id" });
            DropColumn("dbo.ItSystem", "TerminationDeadlineInSystem_Id");
            DropColumn("dbo.ItSystem", "Terminated");
            DropColumn("dbo.ItSystem", "ExpirationDate");
            DropColumn("dbo.ItSystem", "Concluded");
            DropColumn("dbo.ItSystem", "Active");
            DropTable("dbo.TerminationDeadlineTypesInSystems");
        }
    }
}
