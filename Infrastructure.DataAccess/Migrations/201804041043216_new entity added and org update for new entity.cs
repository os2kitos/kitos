namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newentityaddedandorgupdatefornewentity : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Organization", "ContactPersonId", "dbo.User");
            DropIndex("dbo.Organization", new[] { "ContactPersonId" });
            CreateTable(
                "dbo.ContactPersons",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        LastName = c.String(),
                        PhoneNumber = c.String(),
                        Email = c.String(),
                        OrganizationId = c.Int(),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            AddColumn("dbo.Organization", "ContactPerson_Id", c => c.Int());
            CreateIndex("dbo.Organization", "ContactPerson_Id");
            AddForeignKey("dbo.Organization", "ContactPerson_Id", "dbo.ContactPersons", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Organization", "ContactPerson_Id", "dbo.ContactPersons");
            DropForeignKey("dbo.ContactPersons", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ContactPersons", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.ContactPersons", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ContactPersons", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Organization", new[] { "ContactPerson_Id" });
            DropColumn("dbo.Organization", "ContactPerson_Id");
            DropTable("dbo.ContactPersons");
            CreateIndex("dbo.Organization", "ContactPersonId");
            AddForeignKey("dbo.Organization", "ContactPersonId", "dbo.User", "Id");
        }
    }
}
