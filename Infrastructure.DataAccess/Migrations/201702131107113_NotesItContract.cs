namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NotesItContract : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItContractNotes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Note = c.String(),
                        BoolPrivate = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.ItContract", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItContractNotes", "Id", "dbo.ItContract");
            DropForeignKey("dbo.ItContractNotes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItContractNotes", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.ItContractNotes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItContractNotes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItContractNotes", new[] { "Id" });
            DropTable("dbo.ItContractNotes");
        }
    }
}
